using Microsoft.EntityFrameworkCore;
using UrlShortenerPro.Infrastructure.Models;

namespace UrlShortenerPro.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Url> Urls { get; set; }
    public DbSet<ClickData> ClickData { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ClientUsage> ClientUsages { get; set; }
        
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
            
        modelBuilder.Entity<Url>()
            .HasIndex(u => u.ShortCode)
            .IsUnique();
            
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
                
        modelBuilder.Entity<Url>()
            .HasMany(u => u.Clicks)
            .WithOne(c => c.Url)
            .HasForeignKey(c => c.UrlId);
                
        modelBuilder.Entity<User>()
            .HasMany(u => u.Urls)
            .WithOne(url => url.User)
            .HasForeignKey(url => url.UserId);
            
        modelBuilder.Entity<ClientUsage>()
            .HasKey(c => c.Id);
            
        modelBuilder.Entity<ClientUsage>()
            .HasIndex(c => c.ClientId)
            .IsUnique();
            
        modelBuilder.Entity<ClientUsage>()
            .Property(c => c.ClientId)
            .IsRequired()
            .HasMaxLength(50);
    }
}