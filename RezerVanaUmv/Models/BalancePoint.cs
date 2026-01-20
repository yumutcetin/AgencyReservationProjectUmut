
using System;
using System.Collections.Generic;

namespace RezerVanaUmv.Models;

public class BalancePoint
{
    public int Id { get; set; }

    public int? TenantId { get; set; }
    public string? AgencyId { get; set; }
    public string? UserId { get; set; }

    public int? Points { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Agency? Agency { get; set; }

    public virtual Tenant? Tenant { get; set; }
}
