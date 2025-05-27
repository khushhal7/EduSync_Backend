// EduSync/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync.Data;
using EduSync.Models;
using EduSync.DTOs;
using System;
using System.Threading.Tasks;
using BCrypt.Net;
using EduSync.Services; // For IEmailService
using Microsoft.Extensions.Configuration; // For IConfiguration

namespace EduSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EduSyncDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthController(
            EduSyncDbContext context,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegistrationDto userForRegistrationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Users.AnyAsync(u => u.Email == userForRegistrationDto.Email))
            {
                return BadRequest("Email already exists.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userForRegistrationDto.Password);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = userForRegistrationDto.Name,
                Email = userForRegistrationDto.Email,
                PasswordHash = passwordHash,
                Role = userForRegistrationDto.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userToReturn = new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
            return CreatedAtAction(nameof(Register), new { id = user.UserId }, userToReturn);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userForLoginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(userForLoginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials.");
            }

            var userToReturn = new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
            return Ok(userToReturn);
        }

        // POST: api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email);

            if (user != null)
            {
                user.PasswordResetToken = Guid.NewGuid().ToString("N");
                user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

                await _context.SaveChangesAsync();

                var frontendBaseUrl = _configuration["FrontendBaseUrl"];
                if (string.IsNullOrEmpty(frontendBaseUrl))
                {
                    Console.Error.WriteLine("FrontendBaseUrl is not configured.");
                    // Return generic message even if config is missing, to not expose internal issues
                }
                else
                {
                    var resetLink = $"{frontendBaseUrl.TrimEnd('/')}/reset-password/{user.PasswordResetToken}";
                    try
                    {
                        await _emailService.SendPasswordResetEmailAsync(user.Email, user.Name, resetLink);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error sending password reset email: {ex.Message}");
                        // Do not reveal email sending failure to the client
                    }
                }
            }
            return Ok(new { Message = "If an account with that email exists, a password reset link has been sent." });
        }

        // POST: api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // This will include password match errors from DTO
            }

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == resetPasswordDto.Token &&
                u.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                // Token is invalid, not found, or expired
                return BadRequest(new { Message = "Invalid or expired password reset token." });
            }

            // Update the password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);

            // Invalidate the token
            user.PasswordResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Your password has been successfully reset. Please login with your new password." });
        }
    }
}
