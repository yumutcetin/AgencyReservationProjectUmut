using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RezerVanaUmv.Models
{
    public class RoomType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Range(1, 20)]
        public int Capacity { get; set; } = 1;

        [Range(1, 10)]
        public int BedCount { get; set; } = 1;

        [Range(0, 100000)]
        public decimal PricePerNight { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now; // ✅ local time


        // Foreign Key
        public int? TenantId { get; set; }

        [ForeignKey("TenantId")]
        [BindNever]
        public Tenant? Tenant { get; set; }
    }
}
