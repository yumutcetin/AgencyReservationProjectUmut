using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RezerVanaUmv.Models
{
    [Table("reservation_bonus_settings", Schema = "public")]
    public class ReservationBonusSetting
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("bonus_procedure_url")]
        public string? BonusProcedureUrl { get; set; }

        [Column("created_at", TypeName = "timestamp")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("bonus_info_email")]
        [StringLength(200)]
        public string? BonusInfoEmail { get; set; }

        [Column("search_type")]
        [Required]
        [StringLength(50)]
        public string SearchType { get; set; } = "select";

        [Column("min_stay_day")]
        public int? MinStayDay { get; set; } 

        [Column("max_stay_day")]
        public int? MaxStayDay { get; set; } 

        [Column("min_reservation_day")]
        public int MinReservationDay { get; set; } = 0;

        [Column("max_reservation_day")]
        public int MaxReservationDay { get; set; } = 365;

        [Column("min_balance", TypeName = "numeric(10,2)")]
        public decimal MinBalance { get; set; }

        [Column("yearly_use_point")]
        public int YearlyUsePoint { get; set; } = 1000;

        [Column("is_bonus_proc_enabled")]
        public bool IsBonusProcEnabled { get; set; } = false;

        [Column("is_excheckin_date_control")]
        public bool IsExcheckinDateControl { get; set; } = false;

        [Column("tenant_id")]
        public int? TenantId { get; set; }

        [ForeignKey("TenantId")]
        public Tenant? Tenant { get; set; }
    }
}
