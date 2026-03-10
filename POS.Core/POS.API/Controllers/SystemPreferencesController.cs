using Microsoft.AspNetCore.Mvc;
using POS.Application.Interfaces.Services;
using POS.Shared.Models;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/system-preferences")]
    public class SystemPreferencesController : ControllerBase
    {
        private readonly ISystemPreferenceService _service;

        public SystemPreferencesController(ISystemPreferenceService service)
        {
            _service = service;
        }

        // GET: api/system-preferences
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Extract store code from X-Store-Code header with default fallback to 1
                int storeCode = Request.Headers.TryGetValue("X-Store-Code", out var scVal) && int.TryParse(scVal, out int parsedSc) ? parsedSc : 1;
                
                var preferences = await _service.GetByStoreAsync(storeCode);
                
                // If no preferences exist for this store, return default values
                if (preferences == null)
                {
                    return Ok(new SystemPreferenceDto
                    {
                        StoreCode = storeCode,
                        SidebarIdleTimeoutSeconds = 10
                    });
                }
                
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving system preferences", error = ex.Message });
            }
        }

        // PUT: api/system-preferences
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateSystemPreferenceDto dto)
        {
            try
            {
                // Extract store code from X-Store-Code header with default fallback to 1
                int storeCode = Request.Headers.TryGetValue("X-Store-Code", out var scVal) && int.TryParse(scVal, out int parsedSc) ? parsedSc : 1;
                
                var updatedPreferences = await _service.UpdateAsync(storeCode, dto);
                return Ok(updatedPreferences);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating system preferences", error = ex.Message });
            }
        }
    }
}