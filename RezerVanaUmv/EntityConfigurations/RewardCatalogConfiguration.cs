using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class RewardCatalogConfiguration : IEntityTypeConfiguration<RewardCatalog>
    {
        public void Configure(EntityTypeBuilder<RewardCatalog> entity)
        {
            entity.ToTable("reward_catalog");

            entity.HasKey(e => e.Id).HasName("reward_catalog_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id");

            entity.Property(e => e.RequiredPoints)
                .HasColumnName("required_points");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.Property(e => e.RoomType)
                .HasMaxLength(100)
                .HasColumnName("room_type");

            entity.Property(e => e.StartDate)
                .HasColumnType("date")
                .HasColumnName("start_date");

            entity.Property(e => e.EndDate)
                .HasColumnType("date")
                .HasColumnName("end_date");

            entity.Property(e => e.UseEarningPeriod)
                .HasDefaultValue(true)
                .HasColumnName("use_earning_period");

            entity.HasOne(d => d.Tenant)
                .WithMany(p => p.RewardCatalogs)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("reward_catalog_tenant_id_fkey");

            // Optional: add unique index for (room_type, start_date, end_date)
            entity.HasIndex(e => new { e.RoomType, e.StartDate, e.EndDate })
                .IsUnique()
                .HasDatabaseName("uq_reward_catalog_room_period");

            entity.HasIndex(e => e.TenantId)
                .HasDatabaseName("IX_reward_catalog_tenant_id");
        }
    }
}
