using HRM.Data.Entities;

namespace HRM.Repositories.Dtos.Results
{
    public class UserJwt
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Password { get; set; }
        public Role Role { get; set; }
    }
}
