using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.EntityConfigurations
{
    public class FacilityConfiguration : IEntityTypeConfiguration<Facility>
    {
        public void Configure(EntityTypeBuilder<Facility> entity)
        {
            entity.ToTable("facilities");

            entity.HasKey(e => e.Id).HasName("facilities_pkey");

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Name).HasColumnName("name");

            entity.Property(e => e.Department).HasColumnName("department");

            entity.Property(e => e.TenantId).HasColumnName("tenant_id");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
        }
    }
}