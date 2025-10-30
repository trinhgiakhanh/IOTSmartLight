using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRM.Repositories.Dtos.Models
{
	public class MqttCommand
	{
		public string DeviceId { get; set; } = string.Empty;
		public string Payload { get; set; } = string.Empty;
	}
}
