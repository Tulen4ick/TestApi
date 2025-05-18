using TestApi.Services.Interfaces;
using TestApi.Models;
using TestApi.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
namespace TestApi.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    private readonly UserContext _context;
    private readonly PasswordService _passwordService;

    public AuthService(IConfiguration config, UserContext context, PasswordService passwordService)
    {
        _config = config;
        _context = context;
        _passwordService = passwordService;
        CreateAdmin(_context);
    }

    public async Task<User?> AuthenticateUser(AuthDto dto)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Login == dto.Login) ?? throw new KeyNotFoundException("User with that login is not found");
        var isValid = _passwordService.VerifyPassword(user, user.Password, dto.Password);
        return isValid ? user : null;
    }

    public string GenerateJwtToken(User user)
    {
        if (string.IsNullOrEmpty(_config["Jwt:Key"]))
            throw new ArgumentNullException("Jwt configuration is invalid");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creditionals = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, user.Admin == true ? "Admin" : "NotAdmin")
        };
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:ExpireMinutes"])),
            signingCredentials: creditionals);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async void CreateAdmin(UserContext context)
    {
        var InitAdminUser = new User
        {
            Login = "Admin",
            Password = "",
            Name = "InitialAdmin",
            Gender = 2,
            BirthDay = DateTime.UtcNow,
            Admin = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "System"
        };
        InitAdminUser.Password = _passwordService.HashPassword(InitAdminUser, "Admin123");
        await context.Users.AddAsync(InitAdminUser);
        await context.SaveChangesAsync();
    }
}