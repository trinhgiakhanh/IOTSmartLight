using System;
using System.Collections.Generic;

namespace HRM.Data.DbContexts.Entities;

public partial class CommandHistory
{
    public long HistoryId { get; set; }

    public int? DeviceId { get; set; }

    public long? CommandId { get; set; }

    public string? Result { get; set; }

    public string? ResponsePayload { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual DeviceCommand? Command { get; set; }

    public virtual Device? Device { get; set; }
}
