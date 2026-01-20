using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.LastLoginDate)
                  .HasColumnName("LastLoginDate")
                  .HasColumnType("timestamp with time zone");

            builder.Property(u => u.LastPasswordChangeDate)
                   .HasColumnName("LastPasswordChangeDate")
                   .HasColumnType("timestamp with time zone");

        }
    }
}
