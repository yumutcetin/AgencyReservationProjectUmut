using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Models;
using RezerVanaUmv.Data.EntityConfigurations;
using RezerVanaUmv.Data.Configurations;
using RezerVanaUmv.EntityConfigurations;

namespace RezerVanaUmv.Data;

public partial class RzvnUmvUmvKrmnBlzrContext : IdentityDbContext<ApplicationUser, AppUserRoles, string>
{
    public RzvnUmvUmvKrmnBlzrContext() { }

    public RzvnUmvUmvKrmnBlzrContext(DbContextOptions<RzvnUmvUmvKrmnBlzrContext> options)
        : base(options) { }


    public virtual DbSet<Operator> Operators { get; set; }
    public virtual DbSet<Agency> Agencies { get; set; }
    public DbSet<DavetKoduTablosu> DavetKodlari { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }
    public DbSet<ReservationBonusSetting> ReservationBonusSettings { get; set; }
    public virtual DbSet<Campaign> Campaigns { get; set; }
    public virtual DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }
    public virtual DbSet<Passenger> Passengers { get; set; }
    public virtual DbSet<PassengerPoint> PassengerPoints { get; set; }
    public virtual DbSet<BalancePoint> BalancePoints { get; set; }
    public virtual DbSet<PointTransaction> PointTransactions { get; set; }
    public virtual DbSet<Redemption> Redemptions { get; set; }
    public virtual DbSet<Reservation> Reservations { get; set; }
    public virtual DbSet<RewardCatalog> RewardCatalogs { get; set; }
    public virtual DbSet<Tenant> Tenants { get; set; }
    public virtual DbSet<Facility> Facilities { get; set; }
    public virtual DbSet<MenuItem> MenuItems { get; set; }
    public virtual DbSet<Purchase> Purchases { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=46.62.245.239;Port=5432;Database=RzvnUmvUmvKrmnBlzr;Username=postgres;Password=?a24juPOe1295*;Timeout=30;CommandTimeout=180");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OperatorConfiguration());
        modelBuilder.ApplyConfiguration(new IdentityUserClaimConfiguration());
        modelBuilder.ApplyConfiguration(new RoleClaimConfiguration());
        modelBuilder.ApplyConfiguration(new AgencyConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration());
        modelBuilder.ApplyConfiguration(new CampaignConfiguration());
        modelBuilder.ApplyConfiguration(new DavetKoduTablosuConfiguration());
        modelBuilder.ApplyConfiguration(new LoyaltyPointConfiguration());
        modelBuilder.ApplyConfiguration(new PassengerConfiguration());
        modelBuilder.ApplyConfiguration(new PassengerPointConfiguration());
        modelBuilder.ApplyConfiguration(new BalancePointConfiguration());
        modelBuilder.ApplyConfiguration(new PointTransactionConfiguration());
        modelBuilder.ApplyConfiguration(new RedemptionConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationConfiguration());
        modelBuilder.ApplyConfiguration(new ReservationBonusSettingConfiguration());
        modelBuilder.ApplyConfiguration(new RewardCatalogConfiguration());
        modelBuilder.ApplyConfiguration(new RoomTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new FacilityConfiguration());
        modelBuilder.ApplyConfiguration(new MenuItemConfiguration());
        modelBuilder.ApplyConfiguration(new PurchaseConfiguration());

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}