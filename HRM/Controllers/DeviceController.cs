using HRM.Services.MQTT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRM.Apis.Controllers
{
	[Route("api/device")]
	[ApiController]
	public class DeviceController : ControllerBase
	{
		private readonly IDeviceControlService _controlService;
		public DeviceController(IDeviceControlService controlService)
		{
			_controlService = controlService;
		}

		[HttpGet("devices")]
		public async Task<IActionResult> GetAllDevices()
		{
			var result = await _controlService.GetAllDevicesAsync();
			return Ok(result);
		}
	}
}
