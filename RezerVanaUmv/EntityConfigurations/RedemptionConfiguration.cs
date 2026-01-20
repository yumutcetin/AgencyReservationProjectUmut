using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class RedemptionConfiguration : IEntityTypeConfiguration<Redemption>
    {
        public void Configure(EntityTypeBuilder<Redemption> entity)
        {
            entity.ToTable("redemptions");

            entity.HasKey(e => e.Id)
                  .HasName("redemptions_pkey");

            // 🔑 IDENTITY (Npgsql)
            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .ValueGeneratedOnAdd()
                  .UseIdentityByDefaultColumn(); // << ÖNEMLİ

            entity.Property(e => e.TenantId)
                  .HasColumnName("tenant_id");

            entity.Property(e => e.RequiredPoints)
                  .IsRequired()
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

            // 🔗 Tenant ilişkisi
            entity.HasOne(e => e.Tenant)
                  .WithMany(t => t.Redemptions)
                  .HasForeignKey(e => e.TenantId)
                  .HasConstraintName("redemptions_tenant_id_fkey");

            // 🔍 Indexler
            entity.HasIndex(e => e.TenantId)
                  .HasDatabaseName("ix_redemptions_tenant_id");

            // ❗ DÜZELTME: benzersiz dönem anahtarına tenant'ı ekle
            entity.HasIndex(e => new { e.TenantId, e.RoomType, e.StartDate, e.EndDate })
                  .IsUnique()
                  .HasDatabaseName("uq_redemptions_tenant_room_period");
        }
    }
}
