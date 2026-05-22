using MedControl.Api.Data;
using MedControl.Api.Domain;
using MedControl.Api.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedControl.Api.Features.Medications;

public static class MedicationEndpoints
{
    public static IEndpointRouteBuilder MapMedicationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/medications").WithTags("Medications").RequireAuthorization();

        group.MapGet("/", async (AppDbContext db, ClaimsPrincipal user, bool includeInactive = false) =>
        {
            var userId = user.GetUserId();
            var query = db.Medications
                .Include(m => m.Schedules)
                .Where(m => m.UserId == userId)
                .AsNoTracking();

            if (!includeInactive)
            {
                query = query.Where(m => m.IsActive);
            }

            var medications = await query
                .OrderBy(m => m.Name)
                .Select(m => m.ToResponse())
                .ToListAsync();

            return Results.Ok(medications);
        });

        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var medication = await db.Medications
                .Include(m => m.Schedules)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            return medication is null ? Results.NotFound() : Results.Ok(medication.ToResponse());
        });

        group.MapPost("/", async (MedicationRequest request, AppDbContext db, ClaimsPrincipal user) =>
        {
            var validationError = Validate(request);
            if (validationError is not null)
            {
                return Results.BadRequest(new { error = validationError });
            }

            var medication = new Medication();
            medication.UserId = user.GetUserId();
            Apply(request, medication);

            db.Medications.Add(medication);
            await db.SaveChangesAsync();

            return Results.Created($"/api/medications/{medication.Id}", medication.ToResponse());
        });

        group.MapPut("/{id:guid}", async (Guid id, MedicationRequest request, AppDbContext db, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var medication = await db.Medications
                .Include(m => m.Schedules)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (medication is null)
            {
                return Results.NotFound();
            }

            var validationError = Validate(request);
            if (validationError is not null)
            {
                return Results.BadRequest(new { error = validationError });
            }

            Apply(request, medication);
            medication.UpdatedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync();
            return Results.Ok(medication.ToResponse());
        });

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var medication = await db.Medications.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (medication is null)
            {
                return Results.NotFound();
            }

            medication.IsActive = false;
            medication.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return app;
    }

    private static string? Validate(MedicationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return "Informe o nome do medicamento.";
        }

        if (string.IsNullOrWhiteSpace(request.Dosage))
        {
            return "Informe a dosagem.";
        }

        if (request.StockQuantity < 0 || request.LowStockThreshold < 0)
        {
            return "Estoque e limite mínimo não podem ser negativos.";
        }

        if (request.EndDate is not null && request.EndDate < request.StartDate)
        {
            return "A data final deve ser maior ou igual à data inicial.";
        }

        if (request.Schedules.Count == 0)
        {
            return "Cadastre pelo menos um horário.";
        }

        return request.Schedules.Any(s => !TimeOnly.TryParse(s.Time, out _))
            ? "Use horários no formato HH:mm."
            : null;
    }

    private static void Apply(MedicationRequest request, Medication medication)
    {
        medication.Name = request.Name.Trim();
        medication.Dosage = request.Dosage.Trim();
        medication.Instructions = string.IsNullOrWhiteSpace(request.Instructions)
            ? null
            : request.Instructions.Trim();
        medication.StockQuantity = request.StockQuantity;
        medication.LowStockThreshold = request.LowStockThreshold;
        medication.StartDate = request.StartDate;
        medication.EndDate = request.EndDate;
        medication.IsActive = request.IsActive;
        medication.Schedules.Clear();

        foreach (var schedule in request.Schedules)
        {
            medication.Schedules.Add(new MedicationSchedule
            {
                MedicationId = medication.Id,
                Time = TimeOnly.Parse(schedule.Time),
                Days = schedule.Days == ScheduleDays.None ? ScheduleDays.EveryDay : schedule.Days
            });
        }
    }
}
