using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.AuthService.Models;
using POS.AuthService.Repositories;

namespace POS.AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _repository;

        public UsersController(UserRepository repository)
        {
            _repository = repository;
        }

        // ================= GET ALL USERS =================
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            var users = await _repository.GetAllUsersAsync(includeInactive);
            return Ok(users);
        }

        // ================= GET USER BY ID =================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _repository.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });
            return Ok(user);
        }

        // ================= CREATE USER =================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
                return BadRequest(new { message = "Username is required" });

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Password is required" });

            if (dto.Password.Length < 6)
                return BadRequest(new { message = "Password must be at least 6 characters" });

            // Check if username exists
            if (await _repository.UsernameExistsAsync(dto.Username))
                return BadRequest(new { message = "Username already exists" });

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var userId = await _repository.CreateUserAsync(dto, passwordHash);
            var user = await _repository.GetUserByIdAsync(userId);

            return CreatedAtAction(nameof(GetById), new { id = userId }, user);
        }

        // ================= UPDATE USER =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            var existing = await _repository.GetUserByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "User not found" });

            dto.Id = id;
            await _repository.UpdateUserAsync(dto);

            return Ok(new { message = "User updated successfully" });
        }

        // ================= DELETE (SOFT) USER =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _repository.GetUserByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "User not found" });

            await _repository.SoftDeleteUserAsync(id);
            return Ok(new { message = "User disabled successfully" });
        }

        // ================= RESET PASSWORD =================
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
        {
            var existing = await _repository.GetUserByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "User not found" });

            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
                return BadRequest(new { message = "Password must be at least 6 characters" });

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _repository.ResetPasswordAsync(id, passwordHash);

            return Ok(new { message = "Password reset successfully" });
        }
    }
}
