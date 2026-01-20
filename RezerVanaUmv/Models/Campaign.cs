using System;
using System.Collections.Generic;

namespace RezerVanaUmv.Models;

public partial class Campaign
{
    public int Id { get; set; }

    public int? TenantId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public decimal? Multiplier { get; set; }

    public string? TargetRoomType { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual Tenant? Tenant { get; set; }
}
