using System;
using System.Collections.Generic;

namespace HRM.Data.DbContexts.Entities;

public partial class Area
{
    public int AreaId { get; set; }

    public string AreaCode { get; set; } = null!;

    public string AreaName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Route> Routes { get; set; } = new List<Route>();
}
