using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestApi.Models;
using TestApi.Models.DTOs;
using TestApi.Services.Interfaces;
using TestApi.Controllers;

namespace TestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IHttpContextAccessor httpContextAccessor, IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser(UserCreateDto dto)
        {
            try
            {
                var user = await _userService.CreateUserAsync(dto);
                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{login}")]
        public async Task<IActionResult> UpdateUser(string login, UserUpdateDto dto)
        {
            try
            {
                var user = await _userService.UpdateUserInfoAsync(login, dto);
                return Ok(user);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{login}/password")]
        public async Task<IActionResult> ChangePassword(string login, UpdatePasswordDto dto)
        {
            try
            {
                await _userService.ChangePasswordAsync(login, dto);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{oldLogin}/login")]
        public async Task<IActionResult> ChangeLogin(string oldLogin, UpdateLoginDto dto)
        {
            try
            {
                await _userService.ChangeLoginAsync(oldLogin, dto);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Active")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetActiveUsers()
        {
            try
            {
                var users = await _userService.GetActiveUsersAsync();
                return Ok(users);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{login}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserByLogin(string login)
        {
            try
            {
                var user = await _userService.GetUserByLoginAsync(login);
                var response = new
                {
                    user.Name,
                    user.Gender,
                    user.BirthDay,
                    IsActive = user.RevokedOn == null
                };
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{login}/password")]
        public async Task<IActionResult> GetSelfByLoginAndPassword(string login, string password)
        {
            try
            {
                var user = await _userService.AuthenticateAsync(login, password);
                var response = new
                {
                    user.Name,
                    user.Gender,
                    user.BirthDay
                };
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("older-than/{age}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersOlderThan(int age)
        {
            try
            {
                var users = await _userService.GetUsersOlderThanAsync(age);
                return Ok(users);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("soft/{login}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SoftDelete(string login)
        {
            try
            {
                await _userService.SoftDeleteUserAsync(login);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("restore/{login}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreUser(string login)
        {
            try
            {
                await _userService.RestoreUserAsync(login);
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
