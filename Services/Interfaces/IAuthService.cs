using TestApi.Models;
using TestApi.Models.DTOs;
namespace TestApi.Services.Interfaces;

public interface IAuthService
{
    Task<User?> AuthenticateUser(AuthDto dto);
    string GenerateJwtToken(User user);
}