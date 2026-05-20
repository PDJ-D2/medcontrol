using MedControl.Api.Domain;

namespace MedControl.Api.Features.Intakes;

public sealed record TodayIntakeResponse(
    Guid MedicationId,
    string MedicationName,
    string Dosage,
    string? Instructions,
    int StockQuantity,
    DateTime ScheduledFor,
    IntakeStatus? Status,
    DateTimeOffset? RecordedAt,
    string? Note);

public sealed record RecordIntakeRequest(DateTime ScheduledFor, IntakeStatus Status, string? Note);
