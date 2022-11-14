using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPICachingWithRedis.Data;
using WebAPICachingWithRedis.Models;
using WebAPICachingWithRedis.Services;

namespace WebAPICachingWithRedis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DriversController : ControllerBase
    {
        private readonly ILogger<DriversController> _logger;
        private readonly ICacheService _cacheService;
        private readonly AppDbContext _context;

        public DriversController(
            ILogger<DriversController> logger,
            ICacheService cacheService,
            AppDbContext context
            )
        {
            _logger = logger;
            _cacheService = cacheService;
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //check cach data
            var cachData = _cacheService.GetData<IEnumerable<Driver>>("drivers");
            if (cachData != null && cachData.Any())
            {
                return Ok(cachData);
            }

            cachData = await _context.Drivers.ToListAsync();

            // set expiry time
            var expiryTime = DateTimeOffset.Now.AddSeconds(30);
            _cacheService.SetData<IEnumerable<Driver>>("drivers", cachData, expiryTime);

            return Ok(cachData);
        }


        [HttpPost]
        public async Task<IActionResult> Post(Driver driver)
        {
            var addedObj = await _context.Drivers.AddAsync(driver);
            var expiryTime = DateTimeOffset.Now.AddSeconds(30);
            _cacheService.SetData<Driver>($"driver{driver.Id}", addedObj.Entity, expiryTime);

            await _context.SaveChangesAsync();

            return Ok(addedObj.Entity);
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var exists = await _context.Drivers.FindAsync(id);

            if(exists != null)
            {
                _context.Remove(exists);
                _cacheService.RemoveData($"driver{id}");
                await _context.SaveChangesAsync();

                return Ok("success");
            }

            return NotFound($"not found driver with id: {id}");
        }
    }
}
