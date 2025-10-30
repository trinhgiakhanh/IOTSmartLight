using System;
using System.Collections.Generic;

namespace HRM.Data.DbContexts.Entities;

public partial class Cabinet
{
    public int CabinetId { get; set; }

    public string CabinetCode { get; set; } = null!;

    public string CabinetName { get; set; } = null!;

    public int RouteId { get; set; }

    public string? Location { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

    public virtual Route Route { get; set; } = null!;
}
