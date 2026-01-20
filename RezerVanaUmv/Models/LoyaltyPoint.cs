using System;
using System.Collections.Generic;

namespace RezerVanaUmv.Models;

public partial class LoyaltyPoint
{
    public int Id { get; set; }

    public int? TenantId { get; set; }

    public int? ReservationId { get; set; }

    public string? AgencyId { get; set; }

    public int BasePoints { get; set; }

    public int? BonusPoints { get; set; }

    public int? TotalPoints { get; set; }

    public DateTime? CalculatedAt { get; set; }

    public virtual Agency? Agency { get; set; }

    public virtual Reservation? Reservation { get; set; }

    public virtual Tenant? Tenant { get; set; }
}
