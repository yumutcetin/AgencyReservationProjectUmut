using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RezerVanaUmv.Models
{
    public class Purchase
    {
        public string Id { get; set; }

        public string? GuestId { get; set; }

        public string? FacilityId { get; set; }

        public string? MenuItemId { get; set; }

        public string? StaffId { get; set; }

        public int Quantity { get; set; }

        public decimal? Price { get; set; }

        public decimal? TotalAmount { get; set; }

        public DateTime? PurchasedAt { get; set; }

        public int? TenantId { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
