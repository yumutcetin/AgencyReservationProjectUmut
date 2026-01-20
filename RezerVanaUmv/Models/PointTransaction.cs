using System;
using System.Collections.Generic;

namespace RezerVanaUmv.Models;

public partial class PointTransaction
{
    public int Id { get; set; }

    public int? TenantId { get; set; }

    public string? AgencyId { get; set; }

    public string? Type { get; set; }

    public int? Points { get; set; }

    public string? Description { get; set; }

    public int? ReservationId { get; set; }

    public int? RedemptionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Agency? Agency { get; set; }

    public virtual Tenant? Tenant { get; set; }
}
