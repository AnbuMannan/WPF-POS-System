using Dapper;
using POS.AuthService.Entities;
using POS.AuthService.Infrastructure;
using POS.AuthService.Models;

namespace POS.AuthService.Repositories
{
    public class UserRepository
    {
        private readonly DbConnectionFactory _factory;

        public UserRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        // ================= USERS =================

        public async Task<List<UserDto>> GetAllUsersAsync(bool includeInactive = false)
        {
            using var db = _factory.Create();
            var sql = @"
                SELECT u.Id, u.Username, u.Email, u.FullName, u.RoleId, 
                       r.Name as RoleName, u.IsActive, u.LastLogin, u.CreatedAt
                FROM Users u
                LEFT JOIN Roles r ON u.RoleId = r.Id
                WHERE (@includeInactive = 1 OR u.IsActive = 1)
                ORDER BY u.Username";
            
            var users = await db.QueryAsync<UserDto>(sql, new { includeInactive = includeInactive ? 1 : 0 });
            return users.ToList();
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            using var db = _factory.Create();
            var sql = @"
                SELECT u.Id, u.Username, u.Email, u.FullName, u.RoleId, 
                       r.Name as RoleName, u.IsActive, u.LastLogin, u.CreatedAt
                FROM Users u
                LEFT JOIN Roles r ON u.RoleId = r.Id
                WHERE u.Id = @id";
            
            return await db.QueryFirstOrDefaultAsync<UserDto>(sql, new { id });
        }

        public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null)
        {
            using var db = _factory.Create();
            var sql = "SELECT COUNT(*) FROM Users WHERE Username = @username AND (@excludeId IS NULL OR Id != @excludeId)";
            var count = await db.ExecuteScalarAsync<int>(sql, new { username, excludeId });
            return count > 0;
        }

        public async Task<int> CreateUserAsync(CreateUserDto dto, string passwordHash)
        {
            using var db = _factory.Create();
            var sql = @"
                INSERT INTO Users (Username, Email, FullName, PasswordHash, RoleId, IsActive, CreatedAt)
                VALUES (@Username, @Email, @FullName, @PasswordHash, @RoleId, 1, NOW());
                SELECT LAST_INSERT_ID();";
            
            return await db.ExecuteScalarAsync<int>(sql, new
            {
                dto.Username,
                dto.Email,
                dto.FullName,
                PasswordHash = passwordHash,
                dto.RoleId
            });
        }

        public async Task UpdateUserAsync(UpdateUserDto dto)
        {
            using var db = _factory.Create();
            var sql = @"
                UPDATE Users 
                SET Email = @Email, FullName = @FullName, RoleId = @RoleId, 
                    IsActive = @IsActive, UpdatedAt = NOW()
                WHERE Id = @Id";
            
            await db.ExecuteAsync(sql, dto);
        }

        public async Task SoftDeleteUserAsync(int id)
        {
            using var db = _factory.Create();
            await db.ExecuteAsync("UPDATE Users SET IsActive = 0, UpdatedAt = NOW() WHERE Id = @id", new { id });
        }

        public async Task ResetPasswordAsync(int userId, string passwordHash)
        {
            using var db = _factory.Create();
            await db.ExecuteAsync(
                "UPDATE Users SET PasswordHash = @hash, UpdatedAt = NOW() WHERE Id = @id",
                new { hash = passwordHash, id = userId });
        }

        // ================= ROLES =================

        public async Task<List<RoleDto>> GetAllRolesAsync()
        {
            using var db = _factory.Create();
            var roles = await db.QueryAsync<RoleDto>(
                "SELECT Id, Name, Description, ParentRoleId, IsActive FROM Roles WHERE IsActive = 1 ORDER BY Name");
            return roles.ToList();
        }

        public async Task<RoleDto?> GetRoleByIdAsync(int id)
        {
            using var db = _factory.Create();
            return await db.QueryFirstOrDefaultAsync<RoleDto>(
                "SELECT Id, Name, Description, ParentRoleId, IsActive FROM Roles WHERE Id = @id", new { id });
        }

        public async Task<int> CreateRoleAsync(string name, string? description, int? parentRoleId)
        {
            using var db = _factory.Create();
            var sql = @"
                INSERT INTO Roles (Name, Description, ParentRoleId, IsActive, CreatedAt)
                VALUES (@name, @description, @parentRoleId, 1, NOW());
                SELECT LAST_INSERT_ID();";
            
            return await db.ExecuteScalarAsync<int>(sql, new { name, description, parentRoleId });
        }

        public async Task UpdateRoleAsync(int id, string name, string? description)
        {
            using var db = _factory.Create();
            await db.ExecuteAsync(
                "UPDATE Roles SET Name = @name, Description = @description, UpdatedAt = NOW() WHERE Id = @id",
                new { id, name, description });
        }

        // ================= PERMISSIONS =================

        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            using var db = _factory.Create();
            var permissions = await db.QueryAsync<PermissionDto>(
                "SELECT Id, Code, Description, Module FROM Permissions WHERE IsActive = 1 ORDER BY Module, Code");
            return permissions.ToList();
        }

        public async Task<List<PermissionDto>> GetPermissionsByRoleIdAsync(int roleId)
        {
            using var db = _factory.Create();
            var sql = @"
                SELECT p.Id, p.Code, p.Description, p.Module,
                       CASE WHEN rp.Id IS NOT NULL THEN 1 ELSE 0 END as IsAssigned
                FROM Permissions p
                LEFT JOIN RolePermissions rp ON p.Id = rp.PermissionId AND rp.RoleId = @roleId
                WHERE p.IsActive = 1
                ORDER BY p.Module, p.Code";
            
            var permissions = await db.QueryAsync<PermissionDto>(sql, new { roleId });
            return permissions.ToList();
        }

        public async Task UpdateRolePermissionsAsync(int roleId, List<int> permissionIds)
        {
            using var db = _factory.Create();
            await db.OpenAsync();

            using var tx = db.BeginTransaction();
            try
            {
                // Delete existing permissions
                await db.ExecuteAsync("DELETE FROM RolePermissions WHERE RoleId = @roleId", new { roleId }, tx);

                // Insert new permissions
                foreach (var permId in permissionIds)
                {
                    await db.ExecuteAsync(
                        "INSERT INTO RolePermissions (RoleId, PermissionId) VALUES (@roleId, @permId)",
                        new { roleId, permId }, tx);
                }

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }
}
