using HRM.Data.DbContexts.Entities;
using HRM.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HRM.Data.Data
{
    public class SeedData
    {
        public async static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new SmartlightDbContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<SmartlightDbContext>>()))
            {

                
                if (context.Users.Any())
                {
                    //Mới có thể xóa đi thêm lại từ đầu
                    /*
                    context.Admins.RemoveRange();
                    context.SaveChangesAsync();
                    */
                    return;   // DB has been seeded
                }
                await context.Users.AddRangeAsync(
                    new User { Username = "Trinhkhanh337@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123123mm"), Role = "AdminRole" }
                    );
                await context.SaveChangesAsync();          
            }
        }
    }
}
