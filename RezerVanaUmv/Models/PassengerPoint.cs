using System;
using System.Collections.Generic;

namespace RezerVanaUmv.Models;

public partial class PassengerPoint
{
    public int Id { get; set; }

    public int? TenantId { get; set; }

    public int? PassengerId { get; set; }

    public int? ReservationId { get; set; }

    public int Points { get; set; }

    public string? Type { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Passenger? Passenger { get; set; }

    public virtual Reservation? Reservation { get; set; }

    public virtual Tenant? Tenant { get; set; }
}
