using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RezerVanaUmv.Models
{
    public partial class Redemption
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? TenantId { get; set; }

        public int RequiredPoints { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? CreatedAt { get; set; }

        public string? RoomType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool UseEarningPeriod { get; set; } = true;

        public virtual Tenant? Tenant { get; set; }
    }
}
