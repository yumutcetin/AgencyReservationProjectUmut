using System;
using System.Collections.Generic;

namespace RezerVanaUmv.Models;

public partial class Tenant
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Subdomain { get; set; }

    public string? ContactEmail { get; set; }

    public string? LogoUrl { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
    public ICollection<RoomType> RoomTypes { get; set; }

    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

    public virtual ICollection<LoyaltyPoint> LoyaltyPoints { get; set; } = new List<LoyaltyPoint>();

    public virtual ICollection<PassengerPoint> PassengerPoints { get; set; } = new List<PassengerPoint>();

    public virtual ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();

    public virtual ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();

    public virtual ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual ICollection<RewardCatalog> RewardCatalogs { get; set; } = new List<RewardCatalog>();
    public virtual ICollection<BalancePoint> BalancePoints { get; set; } = new List<BalancePoint>();
}
