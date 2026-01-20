using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class PassengerPointConfiguration : IEntityTypeConfiguration<PassengerPoint>
    {
        public void Configure(EntityTypeBuilder<PassengerPoint> entity)
        {
            entity.ToTable("passenger_points");

            entity.HasKey(e => e.Id).HasName("passenger_points_pkey");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.PassengerId).HasColumnName("passenger_id");

            entity.Property(e => e.ReservationId).HasColumnName("reservation_id");

            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.Property(e => e.Points).HasColumnName("points");

            entity.Property(e => e.Type)
                  .HasMaxLength(20)
                  .HasDefaultValueSql("'earned'::character varying")
                  .HasColumnName("type");

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("now()")
                  .HasColumnType("timestamp without time zone")
                  .HasColumnName("created_at");

            entity.HasOne(d => d.Reservation)
                  .WithMany(p => p.PassengerPoints)
                  .HasForeignKey(d => d.ReservationId)
                  .HasConstraintName("passenger_points_reservation_id_fkey");

            entity.HasOne(d => d.Tenant)
                  .WithMany(p => p.PassengerPoints)
                  .HasForeignKey(d => d.TenantId)
                  .HasConstraintName("passenger_points_tenant_id_fkey");
        }
    }
}
