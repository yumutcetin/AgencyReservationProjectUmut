using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class DavetKoduTablosuConfiguration : IEntityTypeConfiguration<DavetKoduTablosu>
    {
        public void Configure(EntityTypeBuilder<DavetKoduTablosu> entity)
        {
            entity.ToTable("DavetKoduTablosu", "public");

            entity.HasKey(e => e.Id).HasName("pk_davet_kodu_tablosu");

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            entity.Property(e => e.Email)
                  .HasColumnName("Email")
                  .HasMaxLength(255);

            entity.Property(e => e.DavetKodu)
                  .HasColumnName("DavetKodu")
                  .HasMaxLength(255);

            entity.Property(e => e.TenantId)
                  .HasColumnName("TenantId");

            entity.Property(e => e.AgencyId)
                  .HasColumnName("AgencyId");

            entity.Property(e => e.RoleId)
                  .HasColumnName("RoleId");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("IsActive")
                .IsRequired();
        }
    }
}
