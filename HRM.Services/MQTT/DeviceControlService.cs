using HRM.Data.DbContexts.Entities;
using HRM.Repositories.Dtos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HRM.Services.MQTT
{
	public interface IDeviceControlService
	{
		Task<object> ControlDeviceAsync(DeviceControlRequest req);
		Task<object> GetAllDevicesAsync();
	}
	public class DeviceControlService : IDeviceControlService
	{
		private readonly SmartlightDbContext _db;
		private readonly MqttService _mqtt;
		private readonly ILogger<DeviceControlService> _logger;

		public DeviceControlService(SmartlightDbContext db, MqttService mqtt, ILogger<DeviceControlService> logger)
		{
			_db = db;
			_mqtt = mqtt;
			_logger = logger;
		}

		public async Task<object> ControlDeviceAsync(DeviceControlRequest req)
		{
			// 1️⃣ Tìm thiết bị
			var device = await _db.Devices.FirstOrDefaultAsync(d => d.DeviceCode == req.DeviceCode);
			if (device == null)
			{
				_logger.LogWarning("⚠️ Không tìm thấy thiết bị {DeviceCode}", req.DeviceCode);
				return new { message = $"Không tìm thấy thiết bị {req.DeviceCode}" };
			}

			// 2️⃣ Xác định loại lệnh
			string commandType = req.IsOn ? "TurnOn" : "TurnOff";
			string topic = $"smartlight/command/{device.DeviceCode.Replace("DEV-", "")}";

			// 3️⃣ Ghi log lệnh vào DB
			var command = new DeviceCommand
			{
				DeviceId = device.DeviceId,
				CommandType = commandType,
				Payload = JsonSerializer.Serialize(new
				{
					action = commandType,
					brightness = req.Brightness ?? 100
				}),
				Status = "Pending",
				SentAt = DateTime.UtcNow
			};

			_db.DeviceCommands.Add(command);
			await _db.SaveChangesAsync();

			// 4️⃣ Publish MQTT
			await _mqtt.PublishAsync(topic, command.Payload);

			_logger.LogInformation("📤 Gửi lệnh {Type} đến {Device} qua topic {Topic}", commandType, device.DeviceCode, topic);

			return new
			{
				message = $"Đã gửi lệnh {commandType} đến {device.DeviceName}",
				topic,
				payload = command.Payload,
				commandId = command.CommandId
			};
		}

		public async Task<object> GetAllDevicesAsync()
		{
			try
			{
				var devices = await _db.Devices
					.Include(d => d.DeviceType)
					.Include(d => d.Cabinet)
					.Select(d => new
					{
						d.DeviceId,
						d.DeviceCode,
						d.DeviceName,
						Type = d.DeviceType != null ? d.DeviceType.TypeName : null,
						Cabinet = d.Cabinet != null ? d.Cabinet.CabinetName : null,
						d.Status,
						d.LastSeen,
						d.Location,
						d.Latitude,
						d.Longitude,
						d.MqttTopic
					})
					.ToListAsync();

				return new
				{
					count = devices.Count,
					devices
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ Error retrieving devices");
				return new { message = "Lỗi khi lấy danh sách thiết bị", error = ex.Message };
			}
		}
	}
}
