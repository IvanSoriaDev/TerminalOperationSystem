using AuthService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<TechnicalClient> TechnicalClients => Set<TechnicalClient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TechnicalClient>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.ClientId).IsUnique();
            entity.Property(item => item.ClientId).IsRequired();
            entity.Property(item => item.ClientSecret).IsRequired();
            entity.Property(item => item.Role).IsRequired();
            entity.Property(item => item.AllowedScopes).IsRequired();
        });
    }
}
