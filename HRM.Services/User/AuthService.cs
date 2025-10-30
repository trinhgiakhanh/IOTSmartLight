using FluentValidation;
using HRM.Apis.Setting;
using HRM.Data.Entities;
using HRM.Repositories.Base;
using HRM.Repositories.Dtos.Models;
using HRM.Repositories.Dtos.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace HRM.Services.User
{
    public static class AuthError
    {
        public const string EMAIL_NOT_CORRECT = "Email đã nhập không tồn tại";
        public const string PASS_NOT_CORRECT = "Mật khẩu đã nhập bị sai";
        public const string FORBIDDEN = "Không đủ quyền hạn để thay đổi .";
    }
    public interface IAuthService
    {
        Task<ApiResponse<string>> AdminLogin(AccountLogin adminLogin); //Chỉ dùng riêng cho admin
        Task<ApiResponse<bool>> ChangeAccountInformation(int employeeId, AccountUpdate accountUpdate); //Dùng cho cả admin và user
        //Task<ApiResponse<AccountInfo>> GetCurrentAccount();
    }
    public class AuthService : IAuthService
    {
        private readonly IBaseRepository<HRM.Data.DbContexts.Entities.User> _adminRepository;
        private readonly IBaseRepository<Employee> _employeeRepository;
        private readonly JwtSetting _jwtServerSetting;
        private readonly IValidator<AccountLogin> _accountLoginValidator;
        private readonly IValidator<AccountUpdate> _accountUpdateValidator;
        public AuthService(
            IBaseRepository<HRM.Data.DbContexts.Entities.User> adminRepository,
            IOptions<JwtSetting> jwtServerSetting,
            IValidator<AccountLogin> accountLoginValidator,
            IBaseRepository<Employee> employeeRepository,
            IValidator<AccountUpdate> accountUpdateValidator
            )
        {
            _adminRepository = adminRepository;
            _jwtServerSetting = jwtServerSetting.Value;
            _accountLoginValidator = accountLoginValidator;
            _employeeRepository = employeeRepository;
            _accountUpdateValidator = accountUpdateValidator;
        }

        public async Task<ApiResponse<bool>> ChangeAccountInformation(int employeeId, AccountUpdate accountUpdate)
        {
            try
            {

				/* Yêu cầu : 
                 * Admin có thể chỉnh sửa tài khoản mật khẩu của user
                 * User chỉ có thể chỉnh sửa tài khoản, mật khẩu của chính bản thân mình .
                 */

				//Check xem có phải admin không ?
				var currentUserRole = _employeeRepository.Context.GetCurrentUserRole();
				var currentUserId = _employeeRepository.Context.GetCurrentUserId();

				var employee = await _employeeRepository
					.GetAllQueryAble()
					.Where(e => e.Id == employeeId)
					.FirstAsync();

				//if (currentUserRole != Role.Admin && currentUserId != employeeId)
				//{
				//	// Nếu không phải admin và không phải chính chủ
				//	return new ApiResponse<bool>
				//	{
				//		IsSuccess = false,
				//		Message = [AuthError.FORBIDDEN]
				//	};
				//}


				//Thay đổi tài khoản, mật khẩu của user
				employee.UserName = accountUpdate.UserName;
                employee.Password = BCrypt.Net.BCrypt.HashPassword(accountUpdate.Password);
                employee.Email = accountUpdate.Email;
                _employeeRepository.Update(employee);
                await _employeeRepository.SaveChangeAsync();

                return new ApiResponse<bool> { IsSuccess = true };


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //public async Task<ApiResponse<AccountInfo>> GetCurrentAccount()
        //{
        //    try
        //    {
        //        var currentRole = _employeeRepository.Context
        //            .GetCurrentUserRole();

        //        var currentId = _employeeRepository.Context
        //            .GetCurrentUserId();
        //        string email = "";
        //        string name = "";
        //        if (currentRole == Role.Admin)
        //        {
        //            var currentUser = await _adminRepository.GetAllQueryAble()
        //                .Where(e => e.UserId == currentId)
        //                .FirstAsync();
        //            name = currentUser.Username;

        //        }
        //        var accountInfo = new AccountInfo
        //        {
        //            Id = currentId,
        //            Role = currentRole,
        //            Name = name,
        //        };
        //        return new ApiResponse<AccountInfo>
        //        {
        //            Metadata = accountInfo,
        //            IsSuccess = true,
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        public async Task<ApiResponse<string>> AdminLogin(AccountLogin adminLogin)
        {
            try
            {
                var resultValidation = _accountLoginValidator.Validate(adminLogin);
                if (!resultValidation.IsValid)
                {
                    return ApiResponse<string>.FailtureValidation(resultValidation.Errors);
                }
                var adminInDb = await _adminRepository.GetAllQueryAble().
                    Where(e => e.Username == adminLogin.Name)
                    .FirstOrDefaultAsync();
                if (adminInDb == null)
                {
                    return new ApiResponse<string> { Message = [AuthError.EMAIL_NOT_CORRECT] };
                }
                bool isCorrectPass = BCrypt.Net.BCrypt.Verify(adminLogin.Password, adminInDb.PasswordHash);
                if (!isCorrectPass)
                {
                    return new ApiResponse<string> { Message = [AuthError.PASS_NOT_CORRECT] };
                }
                string encrypterToken = await JWTGenerator(new UserJwt
                {
                    Name = adminInDb.Username,
                    Password = adminInDb.PasswordHash,
                    Role = Role.Admin,
                    Id = adminInDb.UserId
                }
                );
                return new ApiResponse<string> { Metadata = encrypterToken, IsSuccess = true };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        


        private async Task<string> JWTGenerator(UserJwt userJwt)
        {
            try
            {
                var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Name, _jwtServerSetting.Subject),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                        new Claim("Id", userJwt.Id.ToString()),
                        new Claim("Name", userJwt.Name),
                        new Claim("Password", userJwt.Password),
                        new Claim("Role",userJwt.Role.ToString())
                    };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtServerSetting.Key));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _jwtServerSetting.Issuer,
                    _jwtServerSetting.Audience,
                    claims,
                    expires: DateTime.UtcNow.AddDays(7),
                    signingCredentials: signIn);

                var encrypterToken = new JwtSecurityTokenHandler().WriteToken(token);
                return encrypterToken;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
