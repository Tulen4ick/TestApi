using Microsoft.AspNetCore.Mvc;
using TestApi.Models;
using TestApi.Models.DTOs;

namespace TestApi.Services.Interfaces;

public interface IUserService
{
    Task<User> GetCurrentUserAsync();
    Task<User> CreateUserAsync(UserCreateDto dto);
    Task<User> UpdateUserInfoAsync(string login, UserUpdateDto dto);
    Task ChangePasswordAsync(string login, UpdatePasswordDto dto);
    Task ChangeLoginAsync(string oldLogin, UpdateLoginDto dto);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<User> GetUserByLoginAsync(string login);
    Task<User> AuthenticateAsync(string login, string password);
    Task<IEnumerable<User>> GetUsersOlderThanAsync(int age);
    Task SoftDeleteUserAsync(string login);
    Task RestoreUserAsync(string login);
}