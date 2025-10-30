using System;
using System.Collections.Generic;

namespace HRM.Data.DbContexts.Entities;

public partial class LogEvent
{
    public long LogId { get; set; }

    public string? EventType { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }
}
