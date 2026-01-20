using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> entity)
        {
            entity.ToTable("tenants");

            entity.HasKey(e => e.Id).HasName("tenants_pkey");

            entity.HasIndex(e => e.Subdomain, "tenants_subdomain_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.Name)
                  .HasMaxLength(255)
                  .HasColumnName("name");

            entity.Property(e => e.ContactEmail)
                  .HasMaxLength(255)
                  .HasColumnName("contact_email");

            entity.Property(e => e.Subdomain)
                  .HasMaxLength(100)
                  .HasColumnName("subdomain");

            entity.Property(e => e.LogoUrl)
                  .HasColumnName("logo_url");

            entity.Property(e => e.IsActive)
                  .HasDefaultValue(true)
                  .HasColumnName("is_active");

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("now()")
                  .HasColumnType("timestamp without time zone")
                  .HasColumnName("created_at");
        }
    }
}
