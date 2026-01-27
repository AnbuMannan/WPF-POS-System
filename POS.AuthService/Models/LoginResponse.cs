namespace POS.AuthService.Models
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public UserResponse? User { get; set; }
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public List<string> Permissions { get; set; } = new();
        public bool IsActive { get; set; }
    }
}
