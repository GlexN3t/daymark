using Daymark.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Daymark.Api.Data;

public class DaymarkDbContext : DbContext
{
    public DaymarkDbContext(DbContextOptions<DaymarkDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Email).HasMaxLength(256);
            entity.Property(u => u.DisplayName).HasMaxLength(80);
            entity.Property(u => u.PasswordHash).HasMaxLength(512);
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Habit>(entity =>
        {
            entity.Property(h => h.Name).HasMaxLength(80);
            entity.Property(h => h.Note).HasMaxLength(500);
            entity.Property(h => h.Color).HasMaxLength(9);
            entity.HasIndex(h => h.UserId);
            entity.HasOne(h => h.User)
                .WithMany(u => u.Habits)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.Property(c => c.Note).HasMaxLength(300);
            entity.HasIndex(c => new { c.HabitId, c.Date }).IsUnique();
            entity.HasOne(c => c.Habit)
                .WithMany(h => h.CheckIns)
                .HasForeignKey(c => c.HabitId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
