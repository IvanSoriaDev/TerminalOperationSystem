using ContainerOperationsService.Domain;
using Microsoft.EntityFrameworkCore;

namespace ContainerOperationsService.Data;

public sealed class ContainerDbContext(DbContextOptions<ContainerDbContext> options) : DbContext(options)
{
    public DbSet<ContainerUnit> Containers => Set<ContainerUnit>();
    public DbSet<ContainerOperationEvent> Events => Set<ContainerOperationEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContainerUnit>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.ContainerNumber).IsUnique();
            entity.Property(item => item.ContainerNumber).IsRequired();
            entity.Property(item => item.Status).IsRequired();
        });

        modelBuilder.Entity<ContainerOperationEvent>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.EventType).IsRequired();
            entity.Property(item => item.Notes).IsRequired();
        });
    }
}
