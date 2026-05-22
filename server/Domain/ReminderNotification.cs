namespace MedControl.Api.Domain;

public sealed class ReminderNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MedicationId { get; set; }
    public DateTime ScheduledFor { get; set; }
    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
    public Medication? Medication { get; set; }
}
