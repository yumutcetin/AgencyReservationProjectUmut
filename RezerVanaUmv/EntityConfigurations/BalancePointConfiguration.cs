using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;
using System.Reflection.Emit;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class BalancePointConfiguration : IEntityTypeConfiguration<BalancePoint>
    {
        public void Configure(EntityTypeBuilder<BalancePoint> entity)
        {
            entity.ToTable("balance_points");

            entity.HasKey(e => e.Id).HasName("balance_points_pkey");

            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd(); ;

            entity.Property(e => e.AgencyId)
                .HasColumnName("agency_id");

            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.Property(e => e.Points).HasColumnName("points");

            entity.Property(e => e.Description).HasColumnName("description");

            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.Property(e => e.CreatedAt)
                 .HasDefaultValueSql("now()")
                 .HasColumnType("timestamp without time zone") // 🔁 değiştirildi
                 .HasColumnName("created_at");


            entity.HasOne(d => d.Agency)
                .WithMany(p => p.BalancePoints)
                .HasForeignKey(d => d.AgencyId)
                .HasConstraintName("balance_points_agency_id_fkey");

            entity.HasOne(d => d.Tenant)
                .WithMany(p => p.BalancePoints)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("balance_points_tenant_id_fkey");
        }
    }
}
