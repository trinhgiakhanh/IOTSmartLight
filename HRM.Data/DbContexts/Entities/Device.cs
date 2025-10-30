using System;
using System.Collections.Generic;

namespace HRM.Data.DbContexts.Entities;

public partial class Device
{
    public int DeviceId { get; set; }

    public string DeviceCode { get; set; } = null!;

    public string DeviceName { get; set; } = null!;

    public int? DeviceTypeId { get; set; }

    public int? CabinetId { get; set; }

    public string? MqttTopic { get; set; }

    public string? Location { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public DateTime? InstallDate { get; set; }

    public string? FirmwareVersion { get; set; }

    public string? Status { get; set; }

    public DateTime? LastSeen { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Cabinet? Cabinet { get; set; }

    public virtual ICollection<CommandHistory> CommandHistories { get; set; } = new List<CommandHistory>();

    public virtual ICollection<DeviceCommand> DeviceCommands { get; set; } = new List<DeviceCommand>();

    public virtual ICollection<DeviceStatus> DeviceStatuses { get; set; } = new List<DeviceStatus>();

    public virtual ICollection<DeviceTelemetry> DeviceTelemetries { get; set; } = new List<DeviceTelemetry>();

    public virtual DeviceType? DeviceType { get; set; }
}
