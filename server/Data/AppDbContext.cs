using MedControl.Api.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MedControl.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<MedicationSchedule> MedicationSchedules => Set<MedicationSchedule>();
    public DbSet<MedicationIntakeLog> MedicationIntakeLogs => Set<MedicationIntakeLog>();
    public DbSet<ReminderNotification> ReminderNotifications => Set<ReminderNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.Property(m => m.UserId).HasMaxLength(450).IsRequired();
            entity.Property(m => m.Name).HasMaxLength(120).IsRequired();
            entity.Property(m => m.Dosage).HasMaxLength(80).IsRequired();
            entity.Property(m => m.Instructions).HasMaxLength(500);
            entity.Property(m => m.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasMany(m => m.Schedules)
                .WithOne(s => s.Medication)
                .HasForeignKey(s => s.MedicationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(m => new { m.UserId, m.Name });
        });

        modelBuilder.Entity<MedicationSchedule>(entity =>
        {
            entity.Property(s => s.Time).IsRequired();
            entity.Property(s => s.Days).HasConversion<int>();
            entity.HasIndex(s => new { s.MedicationId, s.Time });
        });

        modelBuilder.Entity<MedicationIntakeLog>(entity =>
        {
            entity.Property(l => l.UserId).HasMaxLength(450).IsRequired();
            entity.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(l => l.Note).HasMaxLength(300);
            entity.HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(l => new { l.UserId, l.MedicationId, l.ScheduledFor }).IsUnique();
        });

        modelBuilder.Entity<ReminderNotification>(entity =>
        {
            entity.HasOne(n => n.Medication)
                .WithMany()
                .HasForeignKey(n => n.MedicationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(n => new { n.MedicationId, n.ScheduledFor }).IsUnique();
        });
    }
}
