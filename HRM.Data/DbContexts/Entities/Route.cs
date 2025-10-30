using System;
using System.Collections.Generic;

namespace HRM.Data.DbContexts.Entities;

public partial class Route
{
    public int RouteId { get; set; }

    public string RouteCode { get; set; } = null!;

    public string RouteName { get; set; } = null!;

    public int AreaId { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Area Area { get; set; } = null!;

    public virtual ICollection<Cabinet> Cabinets { get; set; } = new List<Cabinet>();
}
