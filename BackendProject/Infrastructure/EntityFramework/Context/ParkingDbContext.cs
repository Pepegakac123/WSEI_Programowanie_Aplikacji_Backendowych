using AppCore.Models;
using AppCore.Services;
using Infrastructure.EntityFramework.Entities;
using Infrastructure.Security;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFramework.Context;

public class ParkingDbContext : IdentityDbContext<AppUser,AppRole,string>
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
       optionsBuilder.UseSqlite("Data Source=parking.db");
    }

    public ParkingDbContext()
    {
        
    }

    private readonly ICurrentUserService _currentUserService;

    public ParkingDbContext(DbContextOptions<ParkingDbContext> options, ICurrentUserService currentUserService) 
        : base(options)
    {
        _currentUserService = currentUserService;
    }
    
    public DbSet<CameraCapture>  CameraCapture { get; set; }
    public DbSet<ParkingGate>  ParkingGate { get; set; }
    public DbSet<ParkingSession>   ParkingSession { get; set; }
    public DbSet<ParkingTariff>  ParkingTariff { get; set; }
    public DbSet<Vehicle> Vehicle { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<ParkingSettings> ParkingSettings { get; set; }
    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<AppUser>(entity =>
        {
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
        });
        builder.Entity<AppRole>(entity =>
        {
            entity.Property(r => r.Name).HasMaxLength(20);
        });
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedByUserId ??= _currentUserService.UserId;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}