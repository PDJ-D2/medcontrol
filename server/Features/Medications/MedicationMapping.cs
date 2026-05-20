using MedControl.Api.Domain;

namespace MedControl.Api.Features.Medications;

public static class MedicationMapping
{
    public static MedicationResponse ToResponse(this Medication medication)
    {
        return new MedicationResponse(
            medication.Id,
            medication.Name,
            medication.Dosage,
            medication.Instructions,
            medication.StockQuantity,
            medication.LowStockThreshold,
            medication.StartDate,
            medication.EndDate,
            medication.IsActive,
            medication.StockQuantity <= medication.LowStockThreshold,
            medication.Schedules
                .OrderBy(s => s.Time)
                .Select(s => new MedicationScheduleResponse(s.Id, s.Time.ToString("HH:mm"), s.Days))
                .ToList());
    }
}
