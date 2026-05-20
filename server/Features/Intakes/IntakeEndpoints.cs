using MedControl.Api.Data;
using MedControl.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace MedControl.Api.Features.Intakes;

public static class IntakeEndpoints
{
    public static IEndpointRouteBuilder MapIntakeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/intakes").WithTags("Intakes");

        group.MapGet("/today", async (DateOnly? date, AppDbContext db) =>
        {
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.Today);
            var start = targetDate.ToDateTime(TimeOnly.MinValue);
            var end = targetDate.ToDateTime(TimeOnly.MaxValue);
            var dayFlag = ToScheduleDay(targetDate.DayOfWeek);

            var medications = await db.Medications
                .Include(m => m.Schedules)
                .Where(m => m.IsActive && m.StartDate <= targetDate && (m.EndDate == null || m.EndDate >= targetDate))
                .AsNoTracking()
                .ToListAsync();

            var logs = await db.MedicationIntakeLogs
                .Where(l => l.ScheduledFor >= start && l.ScheduledFor <= end)
                .AsNoTracking()
                .ToListAsync();

            var result = medications
                .SelectMany(m => m.Schedules
                    .Where(s => s.Days.HasFlag(dayFlag))
                    .Select(s =>
                    {
                        var scheduledFor = targetDate.ToDateTime(s.Time);
                        var log = logs.FirstOrDefault(l =>
                            l.MedicationId == m.Id &&
                            l.ScheduledFor == scheduledFor);

                        return new TodayIntakeResponse(
                            m.Id,
                            m.Name,
                            m.Dosage,
                            m.Instructions,
                            m.StockQuantity,
                            scheduledFor,
                            log?.Status,
                            log?.RecordedAt,
                            log?.Note);
                    }))
                .OrderBy(i => i.ScheduledFor)
                .ToList();

            return Results.Ok(result);
        });

        group.MapPost("/medications/{medicationId:guid}", async (
            Guid medicationId,
            RecordIntakeRequest request,
            AppDbContext db) =>
        {
            var medication = await db.Medications.FirstOrDefaultAsync(m => m.Id == medicationId);
            if (medication is null)
            {
                return Results.NotFound();
            }

            var existingLog = await db.MedicationIntakeLogs.FirstOrDefaultAsync(l =>
                l.MedicationId == medicationId && l.ScheduledFor == request.ScheduledFor);

            var wasTaken = existingLog?.Status == IntakeStatus.Taken;

            if (existingLog is null)
            {
                existingLog = new MedicationIntakeLog
                {
                    MedicationId = medicationId,
                    ScheduledFor = request.ScheduledFor
                };
                db.MedicationIntakeLogs.Add(existingLog);
            }

            existingLog.Status = request.Status;
            existingLog.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            existingLog.RecordedAt = DateTimeOffset.UtcNow;

            if (!wasTaken && request.Status == IntakeStatus.Taken && medication.StockQuantity > 0)
            {
                medication.StockQuantity -= 1;
                medication.UpdatedAt = DateTimeOffset.UtcNow;
            }
            else if (wasTaken && request.Status == IntakeStatus.Skipped)
            {
                medication.StockQuantity += 1;
                medication.UpdatedAt = DateTimeOffset.UtcNow;
            }

            await db.SaveChangesAsync();
            return Results.Ok(new { existingLog.Id });
        });

        return app;
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
