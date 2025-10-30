using HRM.Data.DbContexts.Entities;
using HRM.Repositories.Dtos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System.Security.Authentication;
using System.Text.Json;

namespace HRM.Services.MQTT;

public class MqttService : IHostedService
{
	private readonly ILogger<MqttService> _logger;
	private readonly IConfiguration _config;
	private readonly IServiceScopeFactory _scopeFactory;
	private IManagedMqttClient? _client;

	private readonly string _mqttHost;
	private readonly int _mqttPort;
	private readonly string _mqttUsername;
	private readonly string _mqttPassword;

	public MqttService(ILogger<MqttService> logger, IConfiguration config, IServiceScopeFactory scopeFactory)
	{
		_logger = logger;
		_config = config;
		_scopeFactory = scopeFactory;

		_mqttHost = _config["Mqtt:Host"] ?? "e4f5eb6ac3b5482099026506342334c7.s1.eu.hivemq.cloud";
		_mqttPort = int.Parse(_config["Mqtt:Port"] ?? "8883");
		_mqttUsername = _config["Mqtt:Username"] ?? "trinhkhanh";
		_mqttPassword = _config["Mqtt:Password"] ?? "123123Mm";
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		var mqttFactory = new MqttFactory();
		_client = mqttFactory.CreateManagedMqttClient();

		var mqttClientOptions = new MqttClientOptionsBuilder()
			.WithTcpServer(_mqttHost, _mqttPort)
			.WithCredentials(_mqttUsername, _mqttPassword)
			.WithClientId("SmartLightController_" + Guid.NewGuid())
			.WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
			.WithTlsOptions(o =>
			{
				o.WithSslProtocols(SslProtocols.Tls12);
				o.WithCertificateValidationHandler(_ => true);
			})
			.Build();

		var managedOptions = new ManagedMqttClientOptionsBuilder()
			.WithClientOptions(mqttClientOptions)
			.WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
			.Build();

		// 🔍 Theo dõi trạng thái kết nối
		_client.ConnectedAsync += async e =>
		{
			_logger.LogInformation("✅ Connected to HiveMQ Cloud at {Host}:{Port}", _mqttHost, _mqttPort);

			using var scope = _scopeFactory.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<SmartlightDbContext>();
			var topics = db.Devices.Select(d => d.MqttTopic).ToList();

			foreach (var topic in topics)
			{
				await _client.SubscribeAsync(topic);
				_logger.LogInformation("📡 Subscribed to device topic: {Topic}", topic);
			}

			// Thêm kênh chung
			await _client.SubscribeAsync("smartlight/status/#");
			await _client.SubscribeAsync("smartlight/response/#");
		};

		_client.DisconnectedAsync += async e =>
		{
			_logger.LogWarning("⚠️ Disconnected from HiveMQ: {Reason}", e.Reason);
			if (e.Exception != null)
				_logger.LogError(e.Exception, "❌ MQTT disconnect exception");
			await Task.CompletedTask;
		};

		_client.ApplicationMessageReceivedAsync += async e =>
		{
			var topic = e.ApplicationMessage.Topic;
			var payload = e.ApplicationMessage.ConvertPayloadToString();
			_logger.LogInformation("📩 [{Topic}] => {Payload}", topic, payload);

			try
			{
				using var scope = _scopeFactory.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<SmartlightDbContext>();

				// Tìm DeviceId theo topic
				var device = await db.Devices.FirstOrDefaultAsync(d => d.MqttTopic == topic);
				if (device == null)
				{
					_logger.LogWarning("⚠️ Unknown topic: {Topic}", topic);
					return;
				}

				// Lưu payload gốc
				db.DeviceTelemetries.Add(new DeviceTelemetry
				{
					DeviceId = device.DeviceId,
					Payload = payload,
					ReceivedAt = DateTime.UtcNow
				});

				// Giải mã JSON để lưu trạng thái (nếu có)
				if (payload.StartsWith("{"))
				{
					var data = JsonSerializer.Deserialize<DeviceStatusPayload>(payload);
					if (data != null)
					{
						db.DeviceStatuses.Add(new DeviceStatus
						{
							DeviceId = device.DeviceId,
							IsOn = data.IsOn,
							Brightness = data.Brightness,
							Voltage = data.Voltage,
							Current = data.Current,
							Power = data.Power,
							Frequency = data.Frequency,
							Temperature = data.Temperature,
							ReceivedAt = DateTime.UtcNow
						});

						// cập nhật trạng thái thiết bị
						device.Status = "Online";
						device.LastSeen = DateTime.UtcNow;
					}
				}

				if (topic.StartsWith("smartlight/response/"))
				{
					var res = JsonSerializer.Deserialize<DeviceCommandResponse>(payload);
					if (res != null)
					{
						db.CommandHistories.Add(new CommandHistory
						{
							DeviceId = device.DeviceId,
							CommandId = res.CommandId,
							Result = res.Status,
							ResponsePayload = payload,
							CreatedAt = DateTime.UtcNow
						});

						// cập nhật trạng thái lệnh
						var cmd = await db.DeviceCommands.FindAsync(res.CommandId);
						if (cmd != null)
						{
							cmd.Status = res.Status;
							cmd.AcknowledgedAt = DateTime.UtcNow;
						}

						await db.SaveChangesAsync();
					}
				}


				await db.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "❌ Error handling MQTT message");
			}
		};


		try
		{
			_logger.LogInformation("🚀 Connecting to HiveMQ Cloud ({Host}:{Port}) ...", _mqttHost, _mqttPort);
			await _client.StartAsync(managedOptions);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "❌ Failed to connect to HiveMQ Cloud.");
		}
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		if (_client != null)
		{
			await _client.StopAsync();
			_logger.LogInformation("🛑 MQTT client stopped.");
		}
	}

	public async Task PublishAsync(string topic, string payload)
	{
		if (_client == null)
		{
			_logger.LogWarning("⚠️ MQTT client is not initialized.");
			return;
		}

		var message = new MqttApplicationMessageBuilder()
			.WithTopic(topic)
			.WithPayload(payload)
			.WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
			.Build();

		await _client.EnqueueAsync(message);
		_logger.LogInformation("📤 Published to {Topic}: {Payload}", topic, payload);
	}
}
