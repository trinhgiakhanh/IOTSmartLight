using System;
using System.Collections.Generic;

namespace HRM.Data.DbContexts.Entities;

public partial class DeviceTelemetry
{
    public long TelemetryId { get; set; }

    public int? DeviceId { get; set; }

    public string? Payload { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public virtual Device? Device { get; set; }
}
