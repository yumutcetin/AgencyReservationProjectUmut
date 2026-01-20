using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
    {
        public void Configure(EntityTypeBuilder<Campaign> entity)
        {
            entity.ToTable("campaigns");

            entity.HasKey(e => e.Id).HasName("campaigns_pkey");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Title)
                  .HasMaxLength(255)
                  .HasColumnName("title");

            entity.Property(e => e.Description)
                  .HasColumnName("description");

            entity.Property(e => e.TargetRoomType)
                  .HasMaxLength(100)
                  .HasColumnName("target_room_type");

            entity.Property(e => e.StartDate)
                  .HasColumnName("start_date");

            entity.Property(e => e.EndDate)
                  .HasColumnName("end_date");

            entity.Property(e => e.Multiplier)
                  .HasPrecision(3, 2)
                  .HasDefaultValueSql("1.0")
                  .HasColumnName("multiplier");

            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true)
                  .HasColumnName("is_active");

            entity.Property(e => e.TenantId)
                  .HasColumnName("tenant_id");

            entity.HasOne(d => d.Tenant)
                  .WithMany(p => p.Campaigns)
                  .HasForeignKey(d => d.TenantId)
                  .HasConstraintName("campaigns_tenant_id_fkey");
        }
    }
}
