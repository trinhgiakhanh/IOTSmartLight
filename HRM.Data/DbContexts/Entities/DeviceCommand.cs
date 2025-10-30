using System;
using System.Collections.Generic;

namespace HRM.Data.DbContexts.Entities;

public partial class DeviceCommand
{
    public long CommandId { get; set; }

    public int? DeviceId { get; set; }

    public string? CommandType { get; set; }

    public string? Payload { get; set; }

    public string? Status { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime? AcknowledgedAt { get; set; }

    public virtual ICollection<CommandHistory> CommandHistories { get; set; } = new List<CommandHistory>();

    public virtual Device? Device { get; set; }
}
