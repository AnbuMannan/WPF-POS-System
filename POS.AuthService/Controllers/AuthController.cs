using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.AuthService.Models;
using POS.AuthService.Services;
using System.Security.Claims;

namespace POS.AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly Services.AuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(Services.AuthService authService, ITokenService tokenService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Login with username and password
        /// Steps: 1. Validate credentials, 2. Check license, 3. Get user permissions, 4. Generate tokens, 5. Store refresh token
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(req?.Username) || string.IsNullOrWhiteSpace(req?.Password))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Username and password are required"
                    });
                }

                // Step 1: Validate credentials
                if (!_authService.ValidateUser(req.Username, req.Password))
                {
                    _logger.LogWarning($"Failed login attempt for username: {req.Username}");
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid credentials"
                    });
                }

                var user = _authService.GetUser(req.Username);
                if (user == null || !user.IsActive)
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "User account is inactive"
                    });
                }

                // Step 2: Check license
                if (!_authService.CheckLicenseForThisMachine())
                {
                    _logger.LogWarning("License not valid for this machine");
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "License not valid for this machine"
                    });
                }

                // Step 3: Get user permissions with hierarchy
                var permissions = _authService.GetPermissionsWithHierarchy(user.RoleId);

                // Step 4: Generate tokens
                var tokens = _authService.GenerateTokens(user, permissions);
                var accessToken = tokens.Item1;
                var refreshToken = tokens.Item2;

                // Step 5: Store refresh token
                _authService.SaveRefreshToken(user.Id, refreshToken, 7);

                _logger.LogInformation($"Successful login for user: {user.Username}");

                // Return structured response
                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    User = new UserResponse
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        Permissions = permissions,
                        IsActive = user.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login");
                return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during login. Please try again later."
                });
            }
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req?.AccessToken) || string.IsNullOrWhiteSpace(req?.RefreshToken))
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Access token and refresh token are required"
                    });
                }

                var principal = _tokenService.GetPrincipalFromExpiredToken(req.AccessToken);
                if (principal == null)
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid access token"
                    });
                }

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid token claims"
                    });
                }

                // Validate refresh token
                if (!_authService.ValidateRefreshToken(userId, req.RefreshToken))
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token"
                    });
                }

                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                var user = _authService.GetUser(username ?? "");
                if (user == null)
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                var permissions = _authService.GetPermissionsWithHierarchy(user.RoleId);
                var newTokens = _authService.GenerateTokens(user, permissions);
                var newAccessToken = newTokens.Item1;
                var newRefreshToken = newTokens.Item2;

                // Store new refresh token
                _authService.SaveRefreshToken(user.Id, newRefreshToken, 7);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                    User = new UserResponse
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        Permissions = permissions,
                        IsActive = user.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during token refresh");
                return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during token refresh. Please try again later."
                });
            }
        }

        /// <summary>
        /// Get current user information (requires authentication)
        /// </summary>
        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "User information not found in token"
                    });
                }

                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                var user = _authService.GetUser(username ?? "");
                if (user == null)
                {
                    return NotFound(new LoginResponse
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                var permissions = _authService.GetPermissionsWithHierarchy(user.RoleId);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "User information retrieved",
                    User = new UserResponse
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        Permissions = permissions,
                        IsActive = user.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user information");
                return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving user information"
                });
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
