using System;
using System.Collections.Generic;

namespace HRM.Data.DbContexts.Entities;

public partial class DeviceType
{
    public int DeviceTypeId { get; set; }

    public string TypeCode { get; set; } = null!;

    public string TypeName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
