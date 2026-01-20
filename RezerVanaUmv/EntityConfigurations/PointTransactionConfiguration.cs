using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class PointTransactionConfiguration : IEntityTypeConfiguration<PointTransaction>
    {
        public void Configure(EntityTypeBuilder<PointTransaction> entity)
        {
            entity.ToTable("point_transactions");

            entity.HasKey(e => e.Id).HasName("point_transactions_pkey");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.AgencyId).HasColumnName("agency_id");

            entity.Property(e => e.ReservationId).HasColumnName("reservation_id");

            entity.Property(e => e.RedemptionId).HasColumnName("redemption_id");

            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.Property(e => e.Points).HasColumnName("points");

            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");

            entity.Property(e => e.Description).HasColumnName("description");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Agency)
                .WithMany(p => p.PointTransactions)
                .HasForeignKey(d => d.AgencyId)
                .HasConstraintName("point_transactions_agency_id_fkey");

            entity.HasOne(d => d.Tenant)
                .WithMany(p => p.PointTransactions)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("point_transactions_tenant_id_fkey");
        }
    }
}
