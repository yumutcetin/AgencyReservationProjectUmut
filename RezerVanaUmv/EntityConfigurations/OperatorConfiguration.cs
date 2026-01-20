using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.EntityConfigurations
{
    public class OperatorConfiguration : IEntityTypeConfiguration<Operator>
    {
        public void Configure(EntityTypeBuilder<Operator> builder)
        {
            builder.ToTable("operators", "public");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                   .HasColumnName("id");

            builder.Property(o => o.TenantId)
                   .HasColumnName("tenant_id")
                   .IsRequired();

            builder.Property(o => o.Name)
                   .HasColumnName("name")
                   .HasMaxLength(255)
                   .IsRequired();

            builder.Property(o => o.CreatedAt)
               .HasColumnName("created_at")
               .HasColumnType("timestamp without time zone")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(o => o.UpdatedAt)
                   .HasColumnName("updated_at")
                   .HasColumnType("timestamp without time zone")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");



            // Foreign key (opsiyonel)
            builder.HasOne(o => o.Tenant)
                   .WithMany()
                   .HasForeignKey(o => o.TenantId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
