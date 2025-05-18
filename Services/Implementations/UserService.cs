using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TestApi.Models;
using TestApi.Services.Interfaces;
using TestApi.Models.DTOs;
namespace TestApi.Services.Implementations;


public class UserService : IUserService
{
    private readonly UserContext _context;
    private readonly PasswordService _passwordService;
    IHttpContextAccessor _httpContextAccessor;

    public UserService(UserContext context, PasswordService passwordService, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _passwordService = passwordService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<User> GetCurrentUserAsync()
    {
        var login = _httpContextAccessor.HttpContext?.User.Identity?.Name;
        if (string.IsNullOrEmpty(login))
            throw new UnauthorizedAccessException();
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Login == login && x.RevokedOn == null) ?? throw new KeyNotFoundException();
        return user;
    }

    public async Task<User> CreateUserAsync(UserCreateDto dto)
    {
        if (await _context.Users.AnyAsync(x => x.Login == dto.Login))
            throw new ArgumentException("User with that login is already exists");
        var currentUser = await GetCurrentUserAsync();
        if (currentUser.RevokedOn != null)
            throw new InvalidOperationException("User is deactivated");
        if (!currentUser.Admin && dto.Admin != null)
            throw new UnauthorizedAccessException("the user cannot set the value of the admin field");
        var user = new User
        {
            Login = dto.Login,
            Password = "",
            Name = dto.Name,
            Gender = dto.Gender,
            BirthDay = dto.BirthDay,
            Admin = dto.Admin ?? false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = currentUser.Login
        };
        user.Password = _passwordService.HashPassword(user, dto.Password);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }
    public async Task<User> UpdateUserInfoAsync(string login, UserUpdateDto dto)
    {
        var currentUser = await GetCurrentUserAsync();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login) ?? throw new KeyNotFoundException("User with that login is not found");
        if (!currentUser.Admin && currentUser.Login != login)
            throw new UnauthorizedAccessException();
        if (currentUser.RevokedOn != null)
            throw new InvalidOperationException("User is deactivated");
        user.Name = dto.Name ?? user.Name;
        user.Gender = dto.Gender ?? user.Gender;
        user.BirthDay = dto.BirthDay ?? user.BirthDay;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = currentUser.Login;
        await _context.SaveChangesAsync();
        return user;
    }
    public async Task ChangePasswordAsync(string login, UpdatePasswordDto dto)
    {
        var currentUser = await GetCurrentUserAsync();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login) ?? throw new KeyNotFoundException("User with that login is not found");
        if (!currentUser.Admin && currentUser.Login != login)
            throw new UnauthorizedAccessException();
        if (currentUser.RevokedOn != null)
            throw new InvalidOperationException("User is deactivated");
        user.Password = _passwordService.HashPassword(user, dto.NewPassword);
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = currentUser.Login;
        await _context.SaveChangesAsync();
    }
    public async Task ChangeLoginAsync(string oldLogin, UpdateLoginDto dto)
    {
        var currentUser = await GetCurrentUserAsync();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == oldLogin) ?? throw new KeyNotFoundException("User with that login is not found");
        if (!currentUser.Admin && currentUser.Login != oldLogin)
            throw new UnauthorizedAccessException();
        if (currentUser.RevokedOn != null)
            throw new InvalidOperationException("User is deactivated");
        if (await _context.Users.AnyAsync(u => u.Login == dto.NewLogin))
            throw new ArgumentException("User with that login is already exists");
        user.Login = dto.NewLogin;
        user.ModifiedOn = DateTime.UtcNow;
        user.ModifiedBy = currentUser.Login;
        await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser.RevokedOn != null)
            throw new InvalidOperationException("User is deactivated");
        return await _context.Users.Where(u => u.RevokedOn == null).OrderBy(u => u.CreatedOn).ToListAsync();
    }
    public async Task<User> GetUserByLoginAsync(string login)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser.RevokedOn != null)
            throw new InvalidOperationException("User is deactivated");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login) ?? throw new KeyNotFoundException("User with that login is not found");
        return user;
    }
    public async Task<User> AuthenticateAsync(string login, string password)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser.RevokedOn != null)
            throw new InvalidOperationException("User is deactivated");
        if (currentUser.Login != login || !_passwordService.VerifyPassword(currentUser, currentUser.Password, password))
            throw new ArgumentException("Invalid login or password");
        return currentUser;
    }
    public async Task<IEnumerable<User>> GetUsersOlderThanAsync(int age)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser.RevokedOn != null)
            throw new InvalidOperationException("User is deactivated");
        return await _context.Users.Where(u => u.BirthDay != null && u.BirthDay <= DateTime.Today.AddYears(-age)).ToListAsync();
    }
    public async Task SoftDeleteUserAsync(string login)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser.RevokedOn != null)
            throw new InvalidOperationException("User is deactivated");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login) ?? throw new KeyNotFoundException("User with that login is not found");
        user.RevokedOn = DateTime.UtcNow;
        user.RevokedBy = currentUser.Login;
        await _context.SaveChangesAsync();
    }
    public async Task RestoreUserAsync(string login)
    {
        var currentUser = await GetCurrentUserAsync();
        if (currentUser.RevokedOn != null)
            throw new InvalidOperationException("User is deactivated");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login) ?? throw new KeyNotFoundException("User with that login is not found");
        user.RevokedOn = null;
        user.RevokedBy = null;
        await _context.SaveChangesAsync();
    }
}