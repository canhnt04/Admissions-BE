using Crm.Application.Auth;
using Crm.Domain.Entities;
using Crm.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly CrmDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(CrmDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == request.UserName))
                return BadRequest("Username already exists.");

            PasswordHelper.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = request.UserName,
                // Chuyển Hash array sang string để lưu theo đúng kiểu của entity User
                PasswordHash = Convert.ToBase64String(passwordHash),
                PasswordSalt = passwordSalt,
                FullName = request.FullName,
                Mobile = request.Mobile,
                IdentificationNumber = request.IdentificationNumber,
                Role = request.Role,
                IsActived = true,
                UserInternalId = $"EMP{new Random().Next(1000, 9999)}"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful" });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
            if (user == null || !user.IsActived)
                return Unauthorized("Invalid credentials or inactive user.");

            byte[] storedHash = Convert.FromBase64String(user.PasswordHash);
            
            if (!PasswordHelper.VerifyPasswordHash(request.Password, storedHash, user.PasswordSalt))
                return Unauthorized("Invalid credentials.");

            string token = CreateToken(user);

            return Ok(new AuthResponse
            {
                AccessToken = token,
                FullName = user.FullName,
                Role = user.Role
            });
        }

        [HttpPost("assign-role")]
        public async Task<ActionResult> AssignRole(AssignRoleRequest request)
        {
            // Trong thực tế cần có JWT token của Admin để check quyền trước khi gọi API này
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return NotFound("User not found.");

            user.Role = request.Role;
            user.TeamId = request.TeamId;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Role assigned successfully." });
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}
