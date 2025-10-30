using HRM.Data.Entities.Base;

namespace HRM.Data.Entities
{
    public class Employee : BaseEntities
    {
        public int ContractId { get; set; }                   
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; } // = tiếng việt bỏ dấu + năm sinh + contract id
        public string? Password { get; set; } //Mặc định là 123456
        public StatusEmployee StatusEmployee { get; set; }         
        public string? Avatar { get; set; }
        public ICollection<Advance>? advances { get; set; }
        public ICollection<EmployeeImage>? employeeImages { get; set; }
    }
    public enum StatusEmployee
    {
        OnLeave = 1,
        Active = 2,
        Resigned = 3
    }
}
