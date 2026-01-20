using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RezerVanaUmv.Data.Configurations
{
    public class RoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityRoleClaim<string>> builder)
        {
            builder.ToTable("AspNetRoleClaims");

            // Ensure Id is generated automatically (especially needed for PostgreSQL)
            builder.Property(rc => rc.Id)
                   .ValueGeneratedOnAdd();

            // Optional: Set max lengths or required fields if needed
            builder.Property(rc => rc.ClaimType).HasMaxLength(256);
            builder.Property(rc => rc.ClaimValue).HasMaxLength(256);
        }
    }
}
