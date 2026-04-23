using Microsoft.EntityFrameworkCore;
using YardMovePlanningService.Domain;

namespace YardMovePlanningService.Data;

public sealed class YardDbContext(DbContextOptions<YardDbContext> options) : DbContext(options)
{
    public DbSet<YardMoveJob> YardMoveJobs => Set<YardMoveJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<YardMoveJob>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.JobCode).IsUnique();
            entity.Property(item => item.JobCode).IsRequired();
            entity.Property(item => item.ContainerNumber).IsRequired();
            entity.Property(item => item.Status).IsRequired();
        });
    }
}
