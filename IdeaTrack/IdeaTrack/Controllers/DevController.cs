using IdeaTrack.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdeaTrack.Controllers
{
    /// <summary>
    /// Controller for development/testing utilities
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DevController : ControllerBase
    {
        private readonly DataSeederService _seeder;
        private readonly ILogger<DevController> _logger;

        public DevController(DataSeederService seeder, ILogger<DevController> logger)
        {
            _seeder = seeder;
            _logger = logger;
        }

        /// <summary>
        /// Seed sample data into the database (Development only)
        /// GET: /api/dev/seed
        /// </summary>
        [HttpGet("seed")]
        [AllowAnonymous]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                await _seeder.SeedAllAsync();
                return Ok(new { 
                    success = true, 
                    message = "Sample data seeded successfully!",
                    accounts = new[]
                    {
                        new { username = "admin", password = "123456", role = "Admin" },
                        new { username = "scitech1", password = "123456", role = "SciTech" },
                        new { username = "leader_cntt", password = "123456", role = "FacultyLeader" },
                        new { username = "council1", password = "123456", role = "CouncilMember" },
                        new { username = "author1", password = "123456", role = "Author" }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding data");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
