using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRM.Repositories.Dtos.Models
{
	public class DeviceCommandResponse
	{
		public long? CommandId { get; set; }     // ID của lệnh từ bảng DeviceCommand
		public string? Status { get; set; }      // Success / Failed / Timeout
		public string? Message { get; set; }    // Thông tin chi tiết phản hồi (nếu có)
	}
}
