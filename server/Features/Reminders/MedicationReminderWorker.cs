using MedControl.Api.Data;
using MedControl.Api.Domain;
using MedControl.Api.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;

namespace MedControl.Api.Features.Reminders;

public sealed class MedicationReminderWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<MedicationReminderWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            await SendDueRemindersAsync(stoppingToken);
            await timer.WaitForNextTickAsync(stoppingToken);
        }
    }

    private async Task SendDueRemindersAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            var now = DateTime.Now;
            var targetDate = DateOnly.FromDateTime(now);
            var minuteStart = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            var minuteEnd = minuteStart.AddMinutes(1);
            var dayFlag = ToScheduleDay(now.DayOfWeek);

            var medications = await db.Medications
                .Include(m => m.Schedules)
                .Include(m => m.User)
                .Where(m => m.IsActive && m.StartDate <= targetDate && (m.EndDate == null || m.EndDate >= targetDate))
                .ToListAsync(cancellationToken);

            foreach (var medication in medications)
            {
                foreach (var schedule in medication.Schedules.Where(s => s.Days.HasFlag(dayFlag)))
                {
                    var scheduledFor = targetDate.ToDateTime(schedule.Time);
                    if (scheduledFor < minuteStart || scheduledFor >= minuteEnd)
                    {
                        continue;
                    }

                    var alreadySent = await db.ReminderNotifications.AnyAsync(n =>
                        n.MedicationId == medication.Id && n.ScheduledFor == scheduledFor,
                        cancellationToken);

                    if (alreadySent || medication.User?.Email is null)
                    {
                        continue;
                    }

                    await emailSender.SendAsync(
                        medication.User.Email,
                        $"Hora de tomar {medication.Name}",
                        $"Esta na hora de tomar {medication.Name} ({medication.Dosage}). {medication.Instructions}",
                        cancellationToken);

                    db.ReminderNotifications.Add(new ReminderNotification
                    {
                        MedicationId = medication.Id,
                        ScheduledFor = scheduledFor
                    });
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Erro ao enviar lembretes de medicamentos.");
        }
    }

    private static ScheduleDays ToScheduleDay(DayOfWeek day) => day switch
    {
        DayOfWeek.Sunday => ScheduleDays.Sunday,
        DayOfWeek.Monday => ScheduleDays.Monday,
        DayOfWeek.Tuesday => ScheduleDays.Tuesday,
        DayOfWeek.Wednesday => ScheduleDays.Wednesday,
        DayOfWeek.Thursday => ScheduleDays.Thursday,
        DayOfWeek.Friday => ScheduleDays.Friday,
        DayOfWeek.Saturday => ScheduleDays.Saturday,
        _ => ScheduleDays.None
    };
}
