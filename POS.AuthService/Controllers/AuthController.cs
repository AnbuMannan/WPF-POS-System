using Microsoft.AspNetCore.Mvc;
using POS.AuthService.Services;

namespace POS.AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly Services.AuthService _auth;

        public AuthController(Services.AuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest req)
        {
            if (!_auth.CheckLicenseForThisMachine())
                return Unauthorized("License not valid for this machine");

            if (!_auth.ValidateUser(req.Username, req.Password))
                return Unauthorized("Invalid credentials");

            var user = _auth.GetUser(req.Username);
            var permissions = _auth.GetPermissionsWithHierarchy(user.RoleId);

            return Ok(new
            {
                user.Id,
                user.Username,
                RoleId = user.RoleId,
                Permissions = permissions
            });
        }

    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
