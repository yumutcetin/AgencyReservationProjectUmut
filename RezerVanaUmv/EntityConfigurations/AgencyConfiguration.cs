using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class AgencyConfiguration : IEntityTypeConfiguration<Agency>
    {
        public void Configure(EntityTypeBuilder<Agency> entity)
        {
            entity.ToTable("agencies");

            entity.HasKey(e => e.Id).HasName("agencies_pkey");

            // 🔹 ID artık string; uzunluk ve kolon tipi belirt
            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnType("character varying(50)")
                .HasColumnName("id")
                // Eğer PostgreSQL’de default tanımladıysan aç:
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()::text");
            ;

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.Property(e => e.TaxId)
                .HasMaxLength(50)
                .HasColumnName("tax_id");

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");

            entity.Property(e => e.Address)
                .HasColumnName("address");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("now()");

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            entity.Property(e => e.IletisimAlanKodu)
                .HasMaxLength(10)
                .HasColumnName("iletisim_alan_kodu");

            entity.Property(e => e.IletisimTel)
                .HasMaxLength(50)
                .HasColumnName("iletisim_tel");

            entity.Property(e => e.WhatsappAlanKodu)
                .HasMaxLength(10)
                .HasColumnName("whatsapp_alan_kodu");

            entity.Property(e => e.WhatsappNo)
                .HasMaxLength(50)
                .HasColumnName("whatsapp_no");

            entity.Property(e => e.ViberAlanKodu)
                .HasMaxLength(10)
                .HasColumnName("viber_alan_kodu");

            entity.Property(e => e.ViberNo)
                .HasMaxLength(50)
                .HasColumnName("viber_no");

            entity.Property(e => e.Unvan)
                .HasMaxLength(255)
                .HasColumnName("unvan");

            entity.Property(e => e.Ulke)
                .HasMaxLength(100)
                .HasColumnName("ulke");

            entity.Property(e => e.Sehir)
                .HasMaxLength(100)
                .HasColumnName("sehir");

            entity.Property(e => e.SirketTelAlanKodu)
                .HasMaxLength(10)
                .HasColumnName("sirket_tel_alan_kodu");

            entity.Property(e => e.SirketTel)
                .HasMaxLength(50)
                .HasColumnName("sirket_tel");

            entity.Property(e => e.SirketUlke)
                .HasMaxLength(100)
                .HasColumnName("sirket_ulke");

            entity.Property(e => e.SirketSehir)
                .HasMaxLength(100)
                .HasColumnName("sirket_sehir");

            entity.Property(e => e.SirketAdres)
                .HasColumnName("sirket_adres");
        }
    }
}
