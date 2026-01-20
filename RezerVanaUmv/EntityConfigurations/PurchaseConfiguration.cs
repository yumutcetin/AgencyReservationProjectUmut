using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.EntityConfigurations
{
    public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> entity)
        {
            entity.ToTable("purchases");

            entity.HasKey(e => e.Id).HasName("purchases_pkey");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.GuestId).HasColumnName("guest_id");

            entity.Property(e => e.FacilityId).HasColumnName("facility_id");

            entity.Property(e => e.MenuItemId).HasColumnName("menu_item_id");

            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");

            entity.Property(e => e.Price)
                .HasColumnType("numeric(10,2)")
                .HasColumnName("price");

            entity.Property(e => e.TotalAmount)
                .HasColumnType("numeric(10,2)")
                .HasColumnName("total_amount");

            entity.Property(e => e.PurchasedAt)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("now()")
                .HasColumnName("purchased_at");

            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
        }
    }
}
