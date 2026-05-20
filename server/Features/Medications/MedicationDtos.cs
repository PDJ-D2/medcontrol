using MedControl.Api.Domain;

namespace MedControl.Api.Features.Medications;

public sealed record MedicationResponse(
    Guid Id,
    string Name,
    string Dosage,
    string? Instructions,
    int StockQuantity,
    int LowStockThreshold,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    bool IsLowStock,
    IReadOnlyList<MedicationScheduleResponse> Schedules);

public sealed record MedicationScheduleResponse(Guid Id, string Time, ScheduleDays Days);

public sealed record MedicationRequest(
    string Name,
    string Dosage,
    string? Instructions,
    int StockQuantity,
    int LowStockThreshold,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    IReadOnlyList<MedicationScheduleRequest> Schedules);

public sealed record MedicationScheduleRequest(string Time, ScheduleDays Days);
