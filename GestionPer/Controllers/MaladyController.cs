using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionPer.Models;

namespace GestionPer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaladiesController : ControllerBase
    {
        private readonly PersDbContext _context;

        public MaladiesController(PersDbContext context)
        {
            _context = context;
        }

        // GET: api/Maladies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Malady>>> GetMaladies()
        {
            return await _context.Maladies
                .OrderByDescending(m => m.Id)
                .ToListAsync();
        }

        // GET: api/Maladies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Malady>> GetMalady(int id)
        {
            var malady = await _context.Maladies.FindAsync(id);

            if (malady == null)
            {
                return NotFound("Medical record not found");
            }

            return malady;
        }

        // POST: api/Maladies
        [HttpPost]
        public async Task<ActionResult<Malady>> CreateMalady(MaladyCreateDto maladyDto)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(maladyDto.Description))
            {
                return BadRequest("Description is required");
            }

            var malady = new Malady
            {
                Nom = maladyDto.Nom,
                Prenom = maladyDto.Prenom,
                Role = maladyDto.Role,
                Description = maladyDto.Description
            };

            _context.Maladies.Add(malady);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMalady), new { id = malady.Id }, malady);
        }

        // PUT: api/Maladies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMalady(int id, MaladyUpdateDto maladyDto)
        {
            var malady = await _context.Maladies.FindAsync(id);
            if (malady == null)
            {
                return NotFound("Medical record not found");
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(maladyDto.Description))
            {
                return BadRequest("Description is required");
            }

            // Update properties
            malady.Nom = maladyDto.Nom;
            malady.Prenom = maladyDto.Prenom;
            malady.Role = maladyDto.Role;
            malady.Description = maladyDto.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaladyExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Maladies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMalady(int id)
        {
            var malady = await _context.Maladies.FindAsync(id);
            if (malady == null)
            {
                return NotFound("Medical record not found");
            }

            _context.Maladies.Remove(malady);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Maladies/employee/{nom}/{prenom}
        [HttpGet("employee/{nom}/{prenom}")]
        public async Task<ActionResult<IEnumerable<Malady>>> GetEmployeeMaladies(string nom, string prenom)
        {
            var maladies = await _context.Maladies
                .Where(m => m.Nom == nom && m.Prenom == prenom)
                .OrderByDescending(m => m.Id)
                .ToListAsync();

            return maladies;
        }

        // GET: api/Maladies/role/{role}
        [HttpGet("role/{role}")]
        public async Task<ActionResult<IEnumerable<Malady>>> GetMaladiesByRole(string role)
        {
            var maladies = await _context.Maladies
                .Where(m => m.Role == role)
                .OrderByDescending(m => m.Id)
                .ToListAsync();

            return maladies;
        }

        private bool MaladyExists(int id)
        {
            return _context.Maladies.Any(e => e.Id == id);
        }
    }

    // DTOs for creating and updating maladies
    public class MaladyCreateDto
    {
       
        public string Nom { get; set; }

       
        public string Prenom { get; set; }

        public string Role { get; set; }

      
        public string Description { get; set; }
    }

    public class MaladyUpdateDto
    {
     
        public string Nom { get; set; }

       
        public string Prenom { get; set; }

        public string Role { get; set; }

      
        public string Description { get; set; }
    }
}