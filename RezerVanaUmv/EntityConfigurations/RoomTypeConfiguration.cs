using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
    {
        public void Configure(EntityTypeBuilder<RoomType> entity)
        {
            entity.ToTable("roomtypes", "public");

            entity.HasKey(e => e.Id).HasName("roomtypes_pkey");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100)
                  .HasColumnName("name");

            entity.Property(e => e.Description)
                  .HasColumnName("description");

            entity.Property(e => e.Capacity)
                  .IsRequired()
                  .HasDefaultValue(1)
                  .HasColumnName("capacity");

            entity.Property(e => e.BedCount)
                  .IsRequired()
                  .HasDefaultValue(1)
                  .HasColumnName("bedcount");

            entity.Property(e => e.PricePerNight)
                  .IsRequired()
                  .HasColumnType("numeric(10,2)")
                  .HasColumnName("pricepernight");

            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true)
                  .HasColumnName("isactive");

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("timestamp") // ⬅️ Bu satır önemli!
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .HasColumnName("createdat");


            entity.Property(e => e.TenantId)
                  .HasColumnName("tenant_id");

            entity.HasOne(e => e.Tenant)
                  .WithMany(t => t.RoomTypes)
                  .HasForeignKey(e => e.TenantId)
                  .HasConstraintName("roomtypes_tenants_fk")
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
