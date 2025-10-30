using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRM.Repositories.Dtos.Models
{
	public class DeviceControlRequest
	{
		public string DeviceCode { get; set; } = null!;
		public bool IsOn { get; set; }           // true = bật, false = tắt
		public int? Brightness { get; set; }     // tuỳ chọn: 0–100%
	}
}
