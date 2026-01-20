using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RezerVanaUmv.Models;
public partial class Agency
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)] // DB default üretecek
    public string Id { get; set; } = default!;

    public string? Name { get; set; }

    public string? TaxId { get; set; }

    [Required]
    public string? Email { get; set; }

    public string? Address { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    //public virtual ApplicationUser? User { get; set; }

    // 🟡 Kişisel İletişim
    public string? IletisimAlanKodu { get; set; }
    public string? IletisimTel { get; set; }
    public string? WhatsappAlanKodu { get; set; }
    public string? WhatsappNo { get; set; }
    public string? ViberAlanKodu { get; set; }
    public string? ViberNo { get; set; }


    public string? Unvan { get; set; }
    public string? Ulke { get; set; }
    public string? Sehir { get; set; }

    // 🟡 Şirket Bilgileri

    public string? SirketTelAlanKodu { get; set; }

    public string? SirketTel { get; set; }

 
    public string? SirketUlke { get; set; }


    public string? SirketSehir { get; set; }

    public string? SirketAdres { get; set; }





    public virtual ICollection<LoyaltyPoint> LoyaltyPoints { get; set; } = new List<LoyaltyPoint>();
    public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();
    //public virtual ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<BalancePoint> BalancePoints { get; set; } = new List<BalancePoint>();

}
