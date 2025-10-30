using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRM.Repositories.Dtos.Models
{
	public class DeviceStatusPayload
	{
		public bool? IsOn { get; set; }
		public int? Brightness { get; set; }
		public float? Voltage { get; set; }
		public float? Current { get; set; }
		public float? Power { get; set; }
		public float? Frequency { get; set; }
		public float? Temperature { get; set; }
	}

}
