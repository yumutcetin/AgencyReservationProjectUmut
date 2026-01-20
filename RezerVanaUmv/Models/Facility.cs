using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RezerVanaUmv.Models
{
    public class Facility
    {
        public string Id { get; set; }

        public string? Name { get; set; }

        public string? Department { get; set; }

        public string? TenantId { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
