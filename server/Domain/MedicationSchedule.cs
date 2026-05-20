namespace MedControl.Api.Domain;

public sealed class MedicationSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MedicationId { get; set; }
    public TimeOnly Time { get; set; }
    public ScheduleDays Days { get; set; } = ScheduleDays.EveryDay;
    public Medication? Medication { get; set; }
}
