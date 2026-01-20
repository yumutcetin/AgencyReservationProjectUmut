using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class PassengerConfiguration : IEntityTypeConfiguration<Passenger>
    {
        public void Configure(EntityTypeBuilder<Passenger> entity)
        {
            entity.ToTable("passengers");

            entity.HasKey(e => e.Id).HasName("passengers_pkey");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("first_name");

            entity.Property(e => e.BookingReference)
               .IsRequired()
               .HasMaxLength(100)
               .HasColumnName("booking_reference");

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("last_name");

            entity.Property(e => e.Gender)
                .HasMaxLength(20)
                .HasColumnName("gender");

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");

            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");

            entity.Property(e => e.NationalId)
                .HasMaxLength(50)
                .HasColumnName("national_id");

            entity.Property(e => e.ReservationId)
                .HasMaxLength(50)
                .HasColumnName("reservation_id");

            entity.Property(e => e.BirthDate)
                .HasColumnName("birth_date");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.Property(e => e.TenantId)
                .HasColumnName("tenant_id");

            entity.HasOne(d => d.Tenant)
                .WithMany(p => p.Passengers)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("passengers_tenant_id_fkey");
        }
    }
}
