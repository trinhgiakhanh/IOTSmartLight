using System;
using System.Collections.Generic;

namespace HRM.Data.DbContexts.Entities;

public partial class DeviceStatus
{
    public long DeviceStatusId { get; set; }

    public int? DeviceId { get; set; }

    public bool? IsOn { get; set; }

    public int? Brightness { get; set; }

    public double? Voltage { get; set; }

    public double? Current { get; set; }

    public double? Power { get; set; }

    public double? Frequency { get; set; }

    public double? Temperature { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public virtual Device? Device { get; set; }
}
