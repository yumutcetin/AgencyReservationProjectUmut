using System;
using System.Collections.Generic;

namespace RezerVanaUmv.Models;

public partial class Reservation
{
    public int Id { get; set; }

    public int? TenantId { get; set; }

    public string? AgencyId { get; set; }
    public int? Type { get; set; }

    public string? UserId { get; set; }

    public int? OperatorId { get; set; }
    public Operator? Operator { get; set; }
    public DateOnly? CheckinDate { get; set; }

    public DateOnly? CheckoutDate { get; set; }

    public int? RoomCount { get; set; }

    public string? RoomType { get; set; }

    public int? TotalAmount { get; set; }

    public string? BookingReference { get; set; }

    public DateTime? ReservationDate { get; set; }

    public string? Status { get; set; }

    public virtual Agency? Agency { get; set; }
    public string? Notes { get; set; }
    public int? NightCount { get; set; }
    public virtual ICollection<LoyaltyPoint> LoyaltyPoints { get; set; } = new List<LoyaltyPoint>();

    public virtual ICollection<PassengerPoint> PassengerPoints { get; set; } = new List<PassengerPoint>();
    public virtual ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();

    public virtual Tenant? Tenant { get; set; }
}
