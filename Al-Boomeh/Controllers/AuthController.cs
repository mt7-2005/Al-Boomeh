using Al_Boomeh.Models;
using Al_BoomehDAL.Models;
using Al_BoomehServices.Services;
using BCrypt.Net;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Al_Boomeh.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        
        private readonly UsersService _userService;
        private readonly OtpService _otpSeervice;
        private readonly RefreshTokenService _refreshTokesService;
        private readonly IConfiguration _configuration;

        public AuthController(UsersService usersService,OtpService otpService,RefreshTokenService refreshTokesService,IConfiguration configuration)
        {
            _otpSeervice = otpService;
            _refreshTokesService = refreshTokesService;
            _userService = usersService;
            _configuration = configuration;
        }

        [EnableRateLimiting("AuthLimiter")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            
            var user =await _userService.GetUser(request.Email);


            
            if (user == null)
                return Unauthorized("Invalid credentials");


          
            byte[] salt = Convert.FromBase64String(user.PasswordSalt);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(request.Password))
            {
                Salt = salt,
                DegreeOfParallelism = 4,
                Iterations = 3,
                MemorySize = 65536
            };

            byte[] hash = await argon2.GetBytesAsync(32);

            bool valid = CryptographicOperations.FixedTimeEquals(
                hash,
                Convert.FromBase64String(user.Password)
            );

            
            if (!valid)
                return Unauthorized("Invalid credentials");


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),


                new Claim(ClaimTypes.Email, user.Email),


                new Claim(ClaimTypes.Role, user.Role.ToString()),

            };

            if (user.Role == Al_BoomehDAL.Models.User.UserRole.Partner)
            {
                claims.Add(new Claim("storeId", user.StoreId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));




            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            
            var token = new JwtSecurityToken(
                issuer: "Al-BoomehAPI",
                audience: "Al-BoomehAPIUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(3),
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = GenerateRefreshToken();

           string tokedHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
           DateTime expirationDate = DateTime.UtcNow.AddDays(7);

            await _refreshTokesService.SaveRefreshToken(user.Id, tokedHash, expirationDate);

            return Ok(new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        [EnableRateLimiting("AuthLimiter")]
        [HttpPost("RequestOtp")]
        public async Task<IActionResult> RequestOtp([FromQuery] string phone)
        {
            if (string.IsNullOrEmpty(phone)||phone.Length<10||!phone.All(char.IsDigit)) return BadRequest("Invalid input");

            await _otpSeervice.Request(phone);
            return Ok();
        }

        [EnableRateLimiting("AuthLimiter")]
        [HttpPut("Verify")]
        public async Task<IActionResult> Verify(string phone, string code)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 10 || !phone.All(char.IsDigit)) return BadRequest("Invalid input");

            var result = await _otpSeervice.Verify(phone, code);

            switch (result)
            {
                case OtpVerifyResult.NotFound:
                    return BadRequest(new { message = "Code expired or not found" });
                case OtpVerifyResult.InvalidCode:
                    return  BadRequest(new { message = "Invalid code" });

            }
          
            var user = await _userService.GetUserByPhone(phone);

            var claims = new List<Claim>
           {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),


                new Claim(ClaimTypes.Role, user.Role.ToString()),

            };

            if (user.Role == Al_BoomehDAL.Models.User.UserRole.Customer)
            {

                claims.Add(new Claim("customerId", user.CustomerId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));



            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);



            var token = new JwtSecurityToken(
                issuer: "Al-BoomehAPI",
                audience: "Al-BoomehAPIUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );


            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = GenerateRefreshToken();

            string tokedHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
            DateTime expirationDate = DateTime.UtcNow.AddDays(7);

            await _refreshTokesService.SaveRefreshToken(user.Id, tokedHash, expirationDate);

            return Ok(new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<UserDto?>> Me()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(userId == null)
                return null;

            var user=await _userService.GetUser(Guid.Parse(userId));

            return Ok(user);
        }

        [EnableRateLimiting("AuthLimiter")]
        [HttpPost("refresh-staff")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestStaff request)
        {

            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("Invalid input");

            var user=await _userService.GetUser(request.Email);

            if (user == null)
                return Unauthorized("Invalid refresh request");

            var refreshToken = await _refreshTokesService.GetRefreshToken(user.Id);

            if (refreshToken == null)
                return Unauthorized("Refresh token is revoked");

            if (refreshToken.RefreshTokenRevokedAt != null)
                return Unauthorized("Refresh token is revoked");

            if (refreshToken.ExpiresAtUtc <= DateTime.UtcNow)
                return Unauthorized("Refresh token expired");

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, refreshToken.TokenHash);
            if (!refreshValid)
                return Unauthorized("Invalid refresh token");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            if (user.Role == Al_BoomehDAL.Models.User.UserRole.Partner)
            {
                claims.Add(new Claim("storeId", user.StoreId.Value.ToString()));
            }

            if (user.Role == Al_BoomehDAL.Models.User.UserRole.Customer)
            {

                claims.Add(new Claim("customerId", user.CustomerId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));


            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "Al-BoomehAPI",
                audience: "Al-BoomehAPIUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );
            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var newRefreshToken = GenerateRefreshToken();
            string tokedHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
            DateTime expirationDate = DateTime.UtcNow.AddDays(1);

            await _refreshTokesService.Revoked(user.Id);

            await _refreshTokesService.SaveRefreshToken(user.Id, tokedHash, expirationDate);

            return Ok(new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [EnableRateLimiting("AuthLimiter")]
        [HttpPost("refresh-customer")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestCustomer request)
        {

            if (string.IsNullOrEmpty(request.Phone))
                return BadRequest("Invalid input");

            var user=await _userService.GetUser(request.Phone);

            if (user == null)
                return Unauthorized("Invalid refresh request");

            var refreshToken = await _refreshTokesService.GetRefreshToken(user.Id);

            if (refreshToken == null)
                return Unauthorized("Refresh token is revoked");

            if (refreshToken.RefreshTokenRevokedAt != null)
                return Unauthorized("Refresh token is revoked");

            if (refreshToken.ExpiresAtUtc <= DateTime.UtcNow)
                return Unauthorized("Refresh token expired");

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, refreshToken.TokenHash);
            if (!refreshValid)
                return Unauthorized("Invalid refresh token");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            if (user.Role == Al_BoomehDAL.Models.User.UserRole.Partner)
            {
                claims.Add(new Claim("storeId", user.StoreId.Value.ToString()));
            }

            if (user.Role == Al_BoomehDAL.Models.User.UserRole.Customer)
            {

                claims.Add(new Claim("customerId", user.CustomerId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));


            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "Al-BoomehAPI",
                audience: "Al-BoomehAPIUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );
            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var newRefreshToken = GenerateRefreshToken();
            string tokedHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
            DateTime expirationDate = DateTime.UtcNow.AddDays(1);

            await _refreshTokesService.SaveRefreshToken(user.Id, tokedHash, expirationDate);

            return Ok(new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        [HttpPost("logout-staff")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestStaff request)
        {

            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("Invalid input");

            var user = await _userService.GetUser(request.Email);

            var refreshToken = await _refreshTokesService.GetRefreshToken(user.Id);

            if (refreshToken == null)
                return Ok(); 

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, refreshToken.TokenHash);
            if (!refreshValid)
                return Ok();

            await _refreshTokesService.Revoked(user.Id);
            return Ok("Logged out successfully");
        }
        [HttpPost("logout-customer")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestCustomers request)
        {

            if (string.IsNullOrEmpty(request.Phone))
                return BadRequest("Invalid input");

            var user = await _userService.GetUserByPhone(request.Phone);

            var refreshToken = await _refreshTokesService.GetRefreshToken(user.Id);

            if (refreshToken == null)
                return Ok(); 

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, refreshToken.TokenHash);
            if (!refreshValid)
                return Ok();

            refreshToken.RefreshTokenRevokedAt = DateTime.UtcNow;
            return Ok("Logged out successfully");
        }

        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
