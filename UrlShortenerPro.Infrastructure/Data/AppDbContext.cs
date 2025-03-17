using Microsoft.EntityFrameworkCore;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Url> Urls { get; set; }
    public DbSet<ClickData> ClickData { get; set; }
    public DbSet<ClientUsage> ClientUsages { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure indexes
        modelBuilder.Entity<Url>()
            .HasIndex(u => u.ShortCode)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<ClientUsage>()
            .HasIndex(c => c.ClientId)
            .IsUnique();

        // Configure relationships
        modelBuilder.Entity<Url>()
            .HasOne(u => u.User)
            .WithMany(u => u.Urls)
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ClickData>()
            .HasOne(c => c.Url)
            .WithMany(u => u.ClickData)
            .HasForeignKey(c => c.UrlId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserSession>()
            .HasOne(s => s.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}