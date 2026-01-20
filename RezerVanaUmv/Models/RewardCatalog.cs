using System;
using System.Collections.Generic;

namespace RezerVanaUmv.Models
{
    public partial class RewardCatalog
    {
        public int Id { get; set; }

        public int? TenantId { get; set; }

        public int RequiredPoints { get; set; }

        public bool IsActive { get; set; } = true;
        public bool UseEarningPeriod { get; set; } = true;

        public DateTime? CreatedAt { get; set; }

        public string? RoomType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public virtual Tenant? Tenant { get; set; }

        //public virtual ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
    }
}
