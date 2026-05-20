using MedControl.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace MedControl.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<MedicationSchedule> MedicationSchedules => Set<MedicationSchedule>();
    public DbSet<MedicationIntakeLog> MedicationIntakeLogs => Set<MedicationIntakeLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Medication>(entity =>
        {
            entity.Property(m => m.Name).HasMaxLength(120).IsRequired();
            entity.Property(m => m.Dosage).HasMaxLength(80).IsRequired();
            entity.Property(m => m.Instructions).HasMaxLength(500);
            entity.Property(m => m.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasMany(m => m.Schedules)
                .WithOne(s => s.Medication)
                .HasForeignKey(s => s.MedicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MedicationSchedule>(entity =>
        {
            entity.Property(s => s.Time).IsRequired();
            entity.Property(s => s.Days).HasConversion<int>();
            entity.HasIndex(s => new { s.MedicationId, s.Time });
        });

        modelBuilder.Entity<MedicationIntakeLog>(entity =>
        {
            entity.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(l => l.Note).HasMaxLength(300);
            entity.HasIndex(l => new { l.MedicationId, l.ScheduledFor });
        });
    }
}
