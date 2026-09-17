using Al_Boomeh.Models;
using Al_BoomehDAL.Interfaces;
using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Al_BoomehServices.Interfaces;
using Al_BoomehServices.Services;
using BCrypt.Net;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Al_BoomehServices.Services.OtpService;


namespace Al_Boomeh.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        
        private readonly IUsersService _userService;
        private readonly IOtpService _otpSeervice;
        private readonly IRefreshTokenService _refreshTokesService;
        private readonly IConfiguration _configuration;
        private readonly ICurrentUser _currentUser;

        public AuthController(IUsersService usersService, IOtpService otpService, IRefreshTokenService refreshTokesService,IConfiguration configuration, ICurrentUser currentUser)
        {
            _otpSeervice = otpService;
            _refreshTokesService = refreshTokesService;
            _userService = usersService;
            _currentUser = currentUser;
            _configuration = configuration;
        }

        [EnableRateLimiting("AuthLimiter")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Al_Boomeh.Models.LoginRequest request)
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


            string accessToken = BuildAccessToken(user);
            var refreshToken = GenerateRefreshToken();

           DateTime expirationDate = DateTime.UtcNow.AddDays(7);

            await _refreshTokesService.SaveRefreshToken(user.Id, refreshToken, expirationDate);

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

        //[EnableRateLimiting("AuthLimiter")]
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
                    return BadRequest(new { message = "Invalid code" });
                case OtpVerifyResult.ToManyRequest:
                    return StatusCode(429, new 
                    {
                        message= "To many request"
                    });
                case OtpVerifyResult.NotFoundCustomer:
                    return Ok(new
                    {
                        message = "Phone verified. Registration required.",
                        requiresRegistration = true
                    });
            }



            var user = await _userService.GetUserByPhone(phone);

            

            string accessToken = BuildAccessToken(user);

            var refreshToken = GenerateRefreshToken();

           
            DateTime expirationDate = DateTime.UtcNow.AddDays(7);

            await _refreshTokesService.SaveRefreshToken(user.Id, refreshToken, expirationDate);

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
            if (_currentUser.UserId is null)
                return Unauthorized();

            var user = await _userService.GetUser(_currentUser.UserId.Value);
            return Ok(user);
        }

        
        [EnableRateLimiting("AuthLimiter")]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var stored = await _refreshTokesService.GetByRawToken(request.RefreshToken);

            if (stored is null)
                return Unauthorized("Invalid refresh token");

            if (stored.RefreshTokenRevokedAt != null)
            {
                // This exact token was already used once and rotated away.
                // Someone is replaying a stolen token — kill every session for this user.
               
                
                await _refreshTokesService.RevokeAllForUser(stored.UserId);
                
                return Unauthorized("This refresh token was already used. All sessions for this account have been signed out.");
            }

            if (stored.ExpiresAtUtc <= DateTime.UtcNow)
                return Unauthorized("Refresh token expired");

            var user = await _userService.GetUser(stored.UserId);
            if (user is null)
                return Unauthorized("Invalid refresh request");

            string newAccessToken= BuildAccessToken(user);

            // Rotate: kill this exact row, issue a brand new one.
            await _refreshTokesService.RevokeById(stored.Id);
            var newRefreshToken = GenerateRefreshToken();
            await _refreshTokesService.SaveRefreshToken(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(7));

            return Ok(new TokenResponse { AccessToken = newAccessToken, RefreshToken = newRefreshToken });
        }

        [HttpPost("logout-staff")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestStaff request)
        {

            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("Invalid input");

            var user = await _userService.GetUser(request.Email);

            var refreshToken = await _refreshTokesService.GetByRawToken(request.RefreshToken);

            if (refreshToken == null)
                return Ok();

            

            await _refreshTokesService.RevokeById(refreshToken.Id);
            return Ok("Logged out successfully");
        }
        [HttpPost("logout-customer")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestCustomers request)
        {

            if (string.IsNullOrEmpty(request.Phone))
                return BadRequest("Invalid input");

            var user = await _userService.GetUserByPhone(request.Phone);

            var refreshToken = await _refreshTokesService.GetByRawToken(request.RefreshToken);

            if (refreshToken == null)
                return Ok(); 

            

            await _refreshTokesService.RevokeById(refreshToken.Id);
            return Ok("Logged out successfully");
        }

        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
        private string BuildAccessToken(User user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Role, user.Role.ToString())
    };
            if (user.Role == Al_BoomehDAL.Models.User.UserRole.Partner)
                claims.Add(new Claim("storeId", user.StoreId!.Value.ToString()));
            if (user.Role == Al_BoomehDAL.Models.User.UserRole.Customer)
                claims.Add(new Claim("customerId", user.CustomerId!.Value.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "Al-BoomehAPI",
                audience: "Al-BoomehAPIUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
