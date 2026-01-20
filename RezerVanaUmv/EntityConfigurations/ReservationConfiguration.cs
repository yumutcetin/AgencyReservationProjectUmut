using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> entity)
        {
            entity.ToTable("reservations");

            entity.HasKey(e => e.Id).HasName("reservations_pkey");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.Property(e => e.AgencyId).HasColumnName("agency_id");

            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.Property(e => e.BookingReference)
                .HasMaxLength(100)
                .HasColumnName("booking_reference");

            entity.Property(e => e.CheckinDate).HasColumnName("checkin_date");

            entity.Property(e => e.CheckoutDate).HasColumnName("checkout_date");

            entity.Property(e => e.NightCount).HasColumnName("night_count");

            entity.Property(e => e.RoomCount)
                .HasDefaultValue(1)
                .HasColumnName("room_count");

            entity.Property(e => e.RoomType)
                .HasMaxLength(100)
                .HasColumnName("room_type");

            entity.Property(e => e.TotalAmount)
                .HasColumnName("total_amount");

            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'confirmed'::character varying")
                .HasColumnName("status");

            entity.Property(e => e.Notes)
                .HasMaxLength(1000)
                .HasColumnName("notes");

            entity.Property(e => e.ReservationDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("reservation_date");

            entity.HasOne(d => d.Agency)
                .WithMany(p => p.Reservations)
                .HasForeignKey(d => d.AgencyId)
                .HasConstraintName("reservations_agency_id_fkey");

            entity.HasOne(d => d.Tenant)
                .WithMany(p => p.Reservations)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("reservations_tenant_id_fkey");
        }
    }
}
