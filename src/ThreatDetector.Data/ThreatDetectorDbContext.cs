using Microsoft.EntityFrameworkCore;
using ThreatDetector.Core.Models;
using System.Text.Json;

namespace ThreatDetector.Data;

public class ThreatDetectorDbContext : DbContext
{
    public ThreatDetectorDbContext(DbContextOptions<ThreatDetectorDbContext> options) : base(options)
    {
    }

    public DbSet<ThreatEvent> ThreatEvents { get; set; }
    public DbSet<UserBehaviorEvent> UserBehaviorEvents { get; set; }
    public DbSet<ZeroDayVulnerability> ZeroDayVulnerabilities { get; set; }
    public DbSet<SecurityAlert> SecurityAlerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ThreatEvent configuration
        modelBuilder.Entity<ThreatEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ThreatType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Source).HasMaxLength(255);
            entity.Property(e => e.Target).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.UserId).HasMaxLength(100);
            entity.Property(e => e.MitigationAction).HasMaxLength(500);
            
            // Convert Dictionary to JSON string
            entity.Property(e => e.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            entity.HasIndex(e => e.DetectedAt);
            entity.HasIndex(e => e.ThreatType);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.Status);
        });

        // UserBehaviorEvent configuration
        modelBuilder.Entity<UserBehaviorEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EventType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.DeviceFingerprint).HasMaxLength(255);

            // Convert Dictionary and List to JSON strings
            entity.Property(e => e.EventData)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            entity.Property(e => e.AnomalyReasons)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.IsAnomaly);
            entity.HasIndex(e => e.RiskLevel);
        });

        // ZeroDayVulnerability configuration
        modelBuilder.Entity<ZeroDayVulnerability>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VulnerabilityType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AffectedSystem).HasMaxLength(255);
            entity.Property(e => e.AffectedVersion).HasMaxLength(100);
            entity.Property(e => e.ExploitSignature).HasMaxLength(1000);
            entity.Property(e => e.ImpactDescription).HasMaxLength(1000);
            entity.Property(e => e.CveId).HasMaxLength(50);
            entity.Property(e => e.MitigationStrategy).HasMaxLength(1000);

            // Convert Lists and Dictionary to JSON strings
            entity.Property(e => e.AttackVectors)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.TechnicalDetails)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            entity.Property(e => e.RelatedThreatIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.HasIndex(e => e.FirstDetected);
            entity.HasIndex(e => e.VulnerabilityType);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.Status);
        });

        // SecurityAlert configuration
        modelBuilder.Entity<SecurityAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Source).HasMaxLength(255);
            entity.Property(e => e.AcknowledgedBy).HasMaxLength(100);
            entity.Property(e => e.RelatedThreatId).HasMaxLength(50);
            entity.Property(e => e.RelatedVulnerabilityId).HasMaxLength(50);

            // Convert Lists and Dictionary to JSON strings
            entity.Property(e => e.Context)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            entity.Property(e => e.AffectedSystems)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.Property(e => e.RecommendedActions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
        });
    }
} 