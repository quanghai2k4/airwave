using AirWave.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AirWave.Shared.Data;

public class AqiDbContext : DbContext
{
    public DbSet<AqiData> AqiRecords { get; set; }

    public AqiDbContext(DbContextOptions<AqiDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AqiData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AqiValue).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
            
            // Add index for Timestamp column for better query performance
            entity.HasIndex(e => e.Timestamp).IsDescending();
        });
    }
}
