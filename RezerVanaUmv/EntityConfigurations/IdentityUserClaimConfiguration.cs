using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;

namespace RezerVanaUmv.Data.Configurations
{
    public class IdentityUserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder)
        {
            builder.ToTable("AspNetUserClaims");

            // PK olan Id alanı otomatik arttırılır
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id)
                .ValueGeneratedOnAdd(); // Bu satır çok kritik!
            builder.Property(c => c.UserId).IsRequired();
            builder.Property(c => c.ClaimType).HasMaxLength(500);
            builder.Property(c => c.ClaimValue).HasMaxLength(1000);
        }
    }
}
