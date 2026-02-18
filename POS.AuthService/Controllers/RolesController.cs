using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.AuthService.Models;
using POS.AuthService.Repositories;

namespace POS.AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly UserRepository _repository;

        public RolesController(UserRepository repository)
        {
            _repository = repository;
        }

        // ================= GET ALL ROLES =================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _repository.GetAllRolesAsync();
            return Ok(roles);
        }

        // ================= GET ROLE BY ID =================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _repository.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(new { message = "Role not found" });
            return Ok(role);
        }

        // ================= GET ALL PERMISSIONS =================
        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _repository.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        // ================= GET PERMISSIONS BY ROLE =================
        [HttpGet("{id}/permissions")]
        public async Task<IActionResult> GetPermissionsByRoleId(int id)
        {
            var role = await _repository.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(new { message = "Role not found" });

            var permissions = await _repository.GetPermissionsByRoleIdAsync(id);
            return Ok(new { role, permissions });
        }

        // ================= UPDATE ROLE PERMISSIONS =================
        [HttpPut("{id}/permissions")]
        public async Task<IActionResult> UpdatePermissions(int id, [FromBody] UpdateRolePermissionsDto dto)
        {
            var role = await _repository.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(new { message = "Role not found" });

            await _repository.UpdateRolePermissionsAsync(id, dto.PermissionIds);
            return Ok(new { message = "Permissions updated successfully" });
        }

        // ================= CREATE ROLE =================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Role name is required" });

            var roleId = await _repository.CreateRoleAsync(dto.Name, dto.Description, dto.ParentRoleId);
            var role = await _repository.GetRoleByIdAsync(roleId);

            return CreatedAtAction(nameof(GetById), new { id = roleId }, role);
        }

        // ================= UPDATE ROLE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RoleDto dto)
        {
            var existing = await _repository.GetRoleByIdAsync(id);
            if (existing == null)
                return NotFound(new { message = "Role not found" });

            await _repository.UpdateRoleAsync(id, dto.Name, dto.Description);
            return Ok(new { message = "Role updated successfully" });
        }
    }
}
