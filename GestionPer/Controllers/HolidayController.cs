using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionPer.Models;

namespace GestionPer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HolidaysController : ControllerBase
    {
        private readonly PersDbContext _context;

        public HolidaysController(PersDbContext context)
        {
            _context = context;
        }

        // GET: api/Holidays
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Holiday>>> GetHolidays()
        {
            return await _context.Holidays.ToListAsync();
        }

        // GET: api/Holidays/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Holiday>> GetHoliday(int id)
        {
            var holiday = await _context.Holidays.FindAsync(id);

            if (holiday == null)
            {
                return NotFound("Holiday request not found");
            }

            return holiday;
        }

        // POST: api/Holidays
        [HttpPost]
        public async Task<ActionResult<Holiday>> CreateHoliday(HolidayCreateDto holidayDto)
        {
            // Validate dates
            if (holidayDto.DateFin < holidayDto.DateDebut)
            {
                return BadRequest("End date cannot be earlier than start date");
            }

            // Create new holiday request
            var holiday = new Holiday
            {
                Nom = holidayDto.Nom,
                Prenom = holidayDto.Prenom,
                Role = holidayDto.Role,
                DateDebut = holidayDto.DateDebut.Date, // Store date only
                DateFin = holidayDto.DateFin.Date // Store date only
            };

            // Check for overlapping holidays
            var hasOverlap = await _context.Holidays
                .AnyAsync(h =>
                    h.Nom == holiday.Nom &&
                    h.Prenom == holiday.Prenom &&
                    ((h.DateDebut <= holiday.DateFin && h.DateFin >= holiday.DateDebut) ||
                     (h.DateFin >= holiday.DateDebut && h.DateDebut <= holiday.DateFin)));

            if (hasOverlap)
            {
                return BadRequest("There is already a holiday request for this period");
            }

            _context.Holidays.Add(holiday);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHoliday), new { id = holiday.Id }, holiday);
        }

        // PUT: api/Holidays/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHoliday(int id, HolidayUpdateDto holidayDto)
        {
            var holiday = await _context.Holidays.FindAsync(id);
            if (holiday == null)
            {
                return NotFound("Holiday request not found");
            }

            // Validate dates if they are being updated
            if (holidayDto.DateFin < holidayDto.DateDebut)
            {
                return BadRequest("End date cannot be earlier than start date");
            }

            // Check for overlapping holidays (excluding current holiday)
            var hasOverlap = await _context.Holidays
                .Where(h => h.Id != id)
                .AnyAsync(h =>
                    h.Nom == holidayDto.Nom &&
                    h.Prenom == holidayDto.Prenom &&
                    ((h.DateDebut <= holidayDto.DateFin && h.DateFin >= holidayDto.DateDebut) ||
                     (h.DateFin >= holidayDto.DateDebut && h.DateDebut <= holidayDto.DateFin)));

            if (hasOverlap)
            {
                return BadRequest("There is already a holiday request for this period");
            }

            // Update properties
            holiday.Nom = holidayDto.Nom;
            holiday.Prenom = holidayDto.Prenom;
            holiday.Role = holidayDto.Role;
            holiday.DateDebut = holidayDto.DateDebut.Date;
            holiday.DateFin = holidayDto.DateFin.Date;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HolidayExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Holidays/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var holiday = await _context.Holidays.FindAsync(id);
            if (holiday == null)
            {
                return NotFound("Holiday request not found");
            }

            _context.Holidays.Remove(holiday);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Holidays/employee/{nom}/{prenom}
        [HttpGet("employee/{nom}/{prenom}")]
        public async Task<ActionResult<IEnumerable<Holiday>>> GetEmployeeHolidays(string nom, string prenom)
        {
            var holidays = await _context.Holidays
                .Where(h => h.Nom == nom && h.Prenom == prenom)
                .OrderByDescending(h => h.DateDebut)
                .ToListAsync();

            return holidays;
        }

        private bool HolidayExists(int id)
        {
            return _context.Holidays.Any(e => e.Id == id);
        }
    }



    // DTOs for creating and updating holidays
    public class HolidayCreateDto
    {
       
        public string Nom { get; set; }

        
        public string Prenom { get; set; }

        public string Role { get; set; }

     
        public DateTime DateDebut { get; set; }

       
        public DateTime DateFin { get; set; }
    }

    public class HolidayUpdateDto
    {
      
        public string Nom { get; set; }

    
        public string Prenom { get; set; }

        public string Role { get; set; }

    
        public DateTime DateDebut { get; set; }

      
        public DateTime DateFin { get; set; }
    }
}