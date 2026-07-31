using Auth.Application.Common.Helpers;
using Auth.Domain.Entities;
using Auth.Domain.Errors;
using Auth.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.Common.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IAuthDbContext _context;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(IAuthDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Team)
            .FirstOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);
            
        if (user == null || !user.IsActived)
            throw new UnauthorizedException(AuthErrors.InvalidCredentials);

        byte[] storedHash = Convert.FromBase64String(user.PasswordHash);
        
        if (!PasswordHelper.VerifyPasswordHash(request.Password, storedHash, user.PasswordSalt))
            throw new UnauthorizedException(AuthErrors.InvalidCredentials);

        string token = CreateToken(user);

        return new LoginResponse
        {
            AccessToken = token,
        };
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        if (user.Team?.RoleTeam != null)
        {
            claims.Add(new Claim("RoleTeam", ((int)user.Team.RoleTeam.Value).ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetSection("AppSettings:AccessToken").Value!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
