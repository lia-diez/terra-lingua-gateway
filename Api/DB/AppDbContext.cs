using Api.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.DB;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Username).IsRequired();
            entity.Property(e => e.HashedPwd).IsRequired();
        });

        // ApiKey
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.KeyId);
            entity.Property(e => e.KeyString).HasMaxLength(32).IsRequired();
            entity.Property(e => e.ValidTill).IsRequired();

            entity.HasOne(d => d.User)
                .WithMany(p => p.ApiKeys)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}