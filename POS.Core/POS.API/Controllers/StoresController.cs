using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Domain.Entities;
using POS.Infrastructure.Data;
using POS.Shared.Models;

namespace POS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoresController : ControllerBase
    {
        private readonly PosDbContext _context;

        public StoresController(PosDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoreDto>>> GetStores()
        {
            return await _context.Stores
                .Where(s => s.IsActive)
                .Select(s => new StoreDto
                {
                    StoreCode = s.StoreCode,
                    StoreName = s.StoreName,
                    Address = s.Address,
                    ContactNumber = s.ContactNumber,
                    TaxId = s.TaxId,
                    IsActive = s.IsActive
                })
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<StoreDto>> CreateStore(CreateStoreDto dto)
        {
            var store = new Store
            {
                StoreName = dto.StoreName,
                Address = dto.Address,
                ContactNumber = dto.ContactNumber,
                TaxId = dto.TaxId,
                IsActive = true
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStores), new { id = store.StoreCode }, new StoreDto
            {
                StoreCode = store.StoreCode,
                StoreName = store.StoreName,
                Address = store.Address,
                ContactNumber = store.ContactNumber,
                TaxId = store.TaxId,
                IsActive = store.IsActive
            });
        }

        [HttpPost("sync")]
        public async Task<ActionResult<StoreDto>> SyncStore([FromBody] StoreDto dto)
        {
            var existingStore = await _context.Stores.FirstOrDefaultAsync(s => s.StoreCode == dto.StoreCode);
            if (existingStore != null)
            {
                existingStore.StoreName = dto.StoreName;
                existingStore.Address = dto.Address;
                existingStore.ContactNumber = dto.ContactNumber;
                existingStore.TaxId = dto.TaxId;
                existingStore.IsActive = true;
                _context.Stores.Update(existingStore);
            }
            else
            {
                var newStore = new Store
                {
                    StoreCode = dto.StoreCode,
                    StoreName = dto.StoreName,
                    Address = dto.Address,
                    ContactNumber = dto.ContactNumber,
                    TaxId = dto.TaxId,
                    IsActive = true
                };
                _context.Stores.Add(newStore);
            }

            await _context.SaveChangesAsync();
            return Ok(dto);
        }
    }
}