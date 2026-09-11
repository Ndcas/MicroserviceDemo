using LogService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogService.Infrastructure.Database;

public partial class LogServiceContext : DbContext
{
    public LogServiceContext() { }

    public LogServiceContext(DbContextOptions<LogServiceContext> options) : base(options) { }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<LogLevel> LogLevels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("utf8mb4_0900_ai_ci").HasCharSet("utf8mb4");

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("logs");

            entity.HasIndex(e => e.LevelId, "level_id");

            entity.Property(e => e.Id).HasColumnName("id");

            entity
                .Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");

            entity
                .Property(e => e.CorrelationId)
                .HasMaxLength(36)
                .HasColumnName("correlation_id");

            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");

            entity
                .Property(e => e.Ip)
                .HasMaxLength(15)
                .HasColumnName("ip");

            entity.Property(e => e.LevelId).HasColumnName("level_id");

            entity
                .Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("source");

            entity
                .Property(e => e.Time)
                .HasColumnType("timestamp")
                .HasColumnName("time");

            entity
                .HasOne(d => d.Level)
                .WithMany(p => p.Logs)
                .HasForeignKey(d => d.LevelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("logs_ibfk_1");
        });

        modelBuilder.Entity<LogLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("log_levels");

            entity.Property(e => e.Id).HasColumnName("id");

            entity
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");

            entity
                .Property(e => e.Name)
                .HasMaxLength(20)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
