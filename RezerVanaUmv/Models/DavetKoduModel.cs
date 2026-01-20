using System.ComponentModel.DataAnnotations.Schema;

namespace RezerVanaUmv.Models
{
    [Table("DavetKoduTablosu", Schema = "public")]
    public class DavetKoduTablosu
    {
        [Column("id")]
        public int? Id { get; set; }

        [Column("Email")]
        public string? Email { get; set; }
        [Column("DavetKodu")]
        public string? DavetKodu { get; set; }

        [Column("TenantId")]
        public int? TenantId { get; set; }

        [Column("AgencyId")]
        public string? AgencyId { get; set; }

        [Column("RoleId")]
        public string? RoleId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
