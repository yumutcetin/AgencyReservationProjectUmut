using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class LoyaltyPointConfiguration : IEntityTypeConfiguration<LoyaltyPoint>
    {
        public void Configure(EntityTypeBuilder<LoyaltyPoint> entity)
        {
            entity.ToTable("loyalty_points");

            entity.HasKey(e => e.Id).HasName("loyalty_points_pkey");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.AgencyId).HasColumnName("agency_id");

            entity.Property(e => e.ReservationId).HasColumnName("reservation_id");

            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.Property(e => e.BasePoints).HasColumnName("base_points");

            entity.Property(e => e.BonusPoints)
                  .HasDefaultValue(0)
                  .HasColumnName("bonus_points");

            entity.Property(e => e.CalculatedAt)
                  .HasDefaultValueSql("now()")
                  .HasColumnType("timestamp without time zone")
                  .HasColumnName("calculated_at");

            entity.Property(e => e.TotalPoints)
                  .HasComputedColumnSql("(base_points + bonus_points)", stored: true)
                  .HasColumnName("total_points");

            entity.HasOne(d => d.Agency)
                  .WithMany(p => p.LoyaltyPoints)
                  .HasForeignKey(d => d.AgencyId)
                  .HasConstraintName("loyalty_points_agency_id_fkey");

            entity.HasOne(d => d.Reservation)
                  .WithMany(p => p.LoyaltyPoints)
                  .HasForeignKey(d => d.ReservationId)
                  .HasConstraintName("loyalty_points_reservation_id_fkey");

            entity.HasOne(d => d.Tenant)
                  .WithMany(p => p.LoyaltyPoints)
                  .HasForeignKey(d => d.TenantId)
                  .HasConstraintName("loyalty_points_tenant_id_fkey");
        }
    }
}
