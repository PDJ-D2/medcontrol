namespace MedControl.Api.Domain;

public sealed class MedicationIntakeLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MedicationId { get; set; }
    public DateTime ScheduledFor { get; set; }
    public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
    public IntakeStatus Status { get; set; }
    public string? Note { get; set; }
    public Medication? Medication { get; set; }
}
