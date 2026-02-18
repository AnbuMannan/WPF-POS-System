using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Serilog;

namespace POS.UI.Core.Services
{
    public class UserApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger _logger = Log.ForContext<UserApiService>();

        public UserApiService(HttpClient httpClient)
        {
            _http = httpClient;
        }

        // ================= USERS =================

        public async Task<List<UserDto>> GetAllUsersAsync(bool includeInactive = false)
        {
            try
            {
                var response = await _http.GetAsync($"api/users?includeInactive={includeInactive}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new List<UserDto>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get users");
                return new List<UserDto>();
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            try
            {
                var response = await _http.GetAsync($"api/users/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<UserDto>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get user {Id}", id);
                return null;
            }
        }

        public async Task<(bool Success, string Message)> CreateUserAsync(CreateUserDto dto)
        {
            try
            {
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PostAsync("api/users", content);

                if (response.IsSuccessStatusCode)
                    return (true, "User created successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create user");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            try
            {
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PutAsync($"api/users/{id}", content);

                if (response.IsSuccessStatusCode)
                    return (true, "User updated successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to update user {Id}", id);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(int id)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/users/{id}");

                if (response.IsSuccessStatusCode)
                    return (true, "User disabled successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete user {Id}", id);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(int userId, string newPassword)
        {
            try
            {
                var dto = new { UserId = userId, NewPassword = newPassword };
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PostAsync($"api/users/{userId}/reset-password", content);

                if (response.IsSuccessStatusCode)
                    return (true, "Password reset successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to reset password for user {Id}", userId);
                return (false, ex.Message);
            }
        }

        // ================= ROLES =================

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/roles");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<RoleDto>>() ?? new List<RoleDto>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get roles");
                return new List<RoleDto>();
            }
        }

        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            try
            {
                var response = await _http.GetAsync("api/roles/permissions");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<PermissionDto>>() ?? new List<PermissionDto>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get permissions");
                return new List<PermissionDto>();
            }
        }

        public async Task<RolePermissionsResponse?> GetPermissionsByRoleIdAsync(int roleId)
        {
            try
            {
                var response = await _http.GetAsync($"api/roles/{roleId}/permissions");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<RolePermissionsResponse>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get permissions for role {Id}", roleId);
                return null;
            }
        }

        public async Task<(bool Success, string Message)> UpdateRolePermissionsAsync(int roleId, List<int> permissionIds)
        {
            try
            {
                var dto = new { RoleId = roleId, PermissionIds = permissionIds };
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PutAsync($"api/roles/{roleId}/permissions", content);

                if (response.IsSuccessStatusCode)
                    return (true, "Permissions updated successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to update permissions for role {Id}", roleId);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> CreateRoleAsync(string name, string? description)
        {
            try
            {
                var dto = new { Name = name, Description = description };
                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PostAsync("api/roles", content);

                if (response.IsSuccessStatusCode)
                    return (true, "Role created successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create role");
                return (false, ex.Message);
            }
        }
    }

    // DTOs for User API
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public int RoleId { get; set; }
    }

    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentRoleId { get; set; }
        public bool IsActive { get; set; }
    }

    public class PermissionDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public bool IsAssigned { get; set; }
    }

    public class RolePermissionsResponse
    {
        public RoleDto? Role { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new();
    }
}
