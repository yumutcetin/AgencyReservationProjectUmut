using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RezerVanaUmv.Models;

namespace RezerVanaUmv.Data.EntityConfigurations
{
    public class ReservationBonusSettingConfiguration : IEntityTypeConfiguration<ReservationBonusSetting>
    {
        public void Configure(EntityTypeBuilder<ReservationBonusSetting> entity)
        {
            entity.ToTable("reservation_bonus_settings", "public");

            // 🔑 Primary Key
            entity.HasKey(e => e.Id).HasName("pk_reservation_bonus_settings");

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            // 📄 Text Fields
            entity.Property(e => e.BonusProcedureUrl)
                  .HasColumnName("bonus_procedure_url");

            entity.Property(e => e.BonusInfoEmail)
                  .HasMaxLength(200)
                  .HasColumnName("bonus_info_email");

            entity.Property(e => e.SearchType)
                  .IsRequired()
                  .HasMaxLength(50)
                  .HasDefaultValue("select")
                  .HasColumnName("search_type");

            // 📆 Timestamps
            entity.Property(e => e.CreatedAt)
                  .HasColumnType("timestamp")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .HasColumnName("created_at");

            // 🔢 Numeric Fields
            entity.Property(e => e.MinBalance)
                  .HasColumnType("numeric(10,2)")
                  .HasDefaultValue(0)
                  .HasColumnName("min_balance");

            entity.Property(e => e.YearlyUsePoint)
                  .HasDefaultValue(1000)
                  .HasColumnName("yearly_use_point");

            entity.Property(e => e.MinStayDay)
                  .HasDefaultValue(1)
                  .HasColumnName("min_stay_day");

            entity.Property(e => e.MaxStayDay)
                  .HasDefaultValue(30)
                  .HasColumnName("max_stay_day");

            entity.Property(e => e.MinReservationDay)
                  .HasDefaultValue(0)
                  .HasColumnName("min_reservation_day");

            entity.Property(e => e.MaxReservationDay)
                  .HasDefaultValue(365)
                  .HasColumnName("max_reservation_day");

            // ✅ Boolean Flags
            entity.Property(e => e.IsBonusProcEnabled)
                  .HasDefaultValue(false)
                  .HasColumnName("is_bonus_proc_enabled");

            entity.Property(e => e.IsExcheckinDateControl)
                  .HasDefaultValue(false)
                  .HasColumnName("is_excheckin_date_control");

            // 🔗 Foreign Key
            entity.Property(e => e.TenantId)
                  .HasColumnName("tenant_id");

            entity.HasOne(e => e.Tenant)
                  .WithMany()
                  .HasForeignKey(e => e.TenantId)
                  .HasConstraintName("reservation_bonus_settings_tenants_fk")
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
