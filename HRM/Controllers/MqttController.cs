using HRM.Repositories.Dtos.Models;
using HRM.Services.MQTT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Apis.Controllers
{
	[Route("api/mqtt")]
	[ApiController]
	public class MqttController : ControllerBase
	{
		private readonly MqttService _mqtt;
		private readonly IDeviceControlService _controlService; 
		public MqttController(MqttService mqtt, IDeviceControlService controlService)
		{
			_mqtt = mqtt;
			_controlService = controlService;
		}

		[HttpPost("publish")]
		public async Task<IActionResult> Publish([FromBody] MqttCommand cmd)
		{
			await _mqtt.PublishAsync($"smartlight/command/{cmd.DeviceId}", cmd.Payload);
			return Ok(new { message = "Command sent" });
		}

		[HttpGet("test")]
		public IActionResult TestConnection()
		{
			return Ok(new
			{
				message = "✅ MQTT service initialized",
				isConnected = _mqtt != null ? "Service injected OK (check logs for real connection)" : "Service not ready"
			});
		}

		[HttpPost("control")]
		public async Task<IActionResult> ControlDevice([FromBody] DeviceControlRequest req)
		{
			var result = await _controlService.ControlDeviceAsync(req);
			return Ok(result);
		}
	}
}
