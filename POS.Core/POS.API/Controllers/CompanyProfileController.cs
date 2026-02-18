using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/company-profile")]
    public class CompanyProfileController : ControllerBase
    {
        private readonly ICompanyProfileService _service;

        public CompanyProfileController(ICompanyProfileService service)
        {
            _service = service;
        }

        // GET: api/company-profile
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var profile = await _service.GetAsync();
            if (profile == null)
            {
                // Return a default empty profile
                return Ok(new CompanyProfileDto
                {
                    Name = "My Company",
                    CurrencySymbol = "₹",
                    CurrencyCode = "INR"
                });
            }
            return Ok(profile);
        }

        // PUT: api/company-profile
        [HttpPut]
        public async Task<IActionResult> Save([FromBody] UpdateCompanyProfileDto dto)
        {
            try
            {
                var profile = await _service.SaveAsync(dto);
                return Ok(profile);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/company-profile/logo
        [HttpPost("logo")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Invalid file type. Allowed: jpg, jpeg, png, gif" });

            // Save file
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logo");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"company_logo{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var logoUrl = $"/uploads/logo/{fileName}";
            return Ok(new { logoUrl });
        }
    }
}
