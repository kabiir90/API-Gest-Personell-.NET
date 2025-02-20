using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using GestionPer.Models;

namespace GestionPer.Controllers
{
    // DTOs
    public class LoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "First name is required")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string Prenom { get; set; } = string.Empty;

        public string? Tele { get; set; }

        public string? Address { get; set; }

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        public string? Photo { get; set; }

        public string? Role { get; set; }
    }

    public class UpdateEmployeeDto
    {
        public string? Nom { get; set; }
        public string? Prenom { get; set; }
        public string? Tele { get; set; }
        public string? Address { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Photo { get; set; }
        public string? Role { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly PersDbContext _context;
        private readonly IConfiguration _configuration;

        public EmployeesController(PersDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Login endpoint
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(LoginDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Username == request.Username);

                if (employee == null)
                    return BadRequest("Invalid username or password");

                // Compare passwords directly
                if (employee.Password != request.Password)
                    return BadRequest("Invalid username or password");

                string token = CreateToken(employee);

                var response = new
                {
                    token = token,
                    role = employee.Role,
                    userId = employee.Id,
                    username = employee.Username,
                    nom = employee.Nom,
                    prenom = employee.Prenom
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // Register endpoint
        [HttpPost("register")]
        public async Task<ActionResult<object>> Register(RegisterDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (await _context.Employees.AnyAsync(e => e.Username == request.Username))
                    return BadRequest("Username already exists");

                // Store password directly without hashing
                var employee = new Employees
                {
                    Nom = request.Nom,
                    Prenom = request.Prenom,
                    Tele = request.Tele ?? string.Empty,
                    Address = request.Address ?? string.Empty,
                    Username = request.Username,
                    Password = request.Password, // Store password directly
                    Photo = request.Photo ?? string.Empty,
                    Role = request.Role ?? "Employee"
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                // Create response without sensitive data
                var response = new
                {
                    id = employee.Id,
                    username = employee.Username,
                    role = employee.Role,
                    nom = employee.Nom,
                    prenom = employee.Prenom
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        // Get all employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetEmployees()
        {
            try
            {
                var employees = await _context.Employees
                    .Select(e => new
                    {
                        e.Id,
                        e.Nom,
                        e.Prenom,
                        e.Tele,
                        e.Address,
                        e.Username,
                        e.Photo,
                        e.Role
                    })
                    .ToListAsync();

                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving employees.");
            }
        }

        // Get employee by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetEmployee(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);

                if (employee == null)
                    return NotFound("Employee not found");

                var response = new
                {
                    employee.Id,
                    employee.Nom,
                    employee.Prenom,
                    employee.Tele,
                    employee.Address,
                    employee.Username,
                    employee.Photo,
                    employee.Role
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the employee.");
            }
        }

        // Update employee
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto request)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);

                if (employee == null)
                    return NotFound("Employee not found");

                // Update only provided fields
                if (!string.IsNullOrEmpty(request.Nom))
                    employee.Nom = request.Nom;
                if (!string.IsNullOrEmpty(request.Prenom))
                    employee.Prenom = request.Prenom;
                if (!string.IsNullOrEmpty(request.Tele))
                    employee.Tele = request.Tele;
                if (!string.IsNullOrEmpty(request.Address))
                    employee.Address = request.Address;
                if (!string.IsNullOrEmpty(request.Username))
                {
                    // Check if new username is already taken by another user
                    if (await _context.Employees.AnyAsync(e => e.Id != id && e.Username == request.Username))
                        return BadRequest("Username already exists");
                    employee.Username = request.Username;
                }
                if (!string.IsNullOrEmpty(request.Password))
                    employee.Password = request.Password; // Store password directly
                if (!string.IsNullOrEmpty(request.Photo))
                    employee.Photo = request.Photo;
                if (!string.IsNullOrEmpty(request.Role))
                    employee.Role = request.Role;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Employee updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the employee.");
            }
        }

        // Delete employee
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);

                if (employee == null)
                    return NotFound("Employee not found");

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Employee deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the employee.");
            }
        }

        // Modify the existing search endpoint
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<object>>> SearchEmployees([FromQuery] string? searchTerm)
        {
            try
            {
                var query = _context.Employees.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower().Trim();
                    query = query.Where(e =>
                        EF.Functions.Like(e.Nom.ToLower(), $"%{searchTerm}%") ||
                        EF.Functions.Like(e.Prenom.ToLower(), $"%{searchTerm}%") ||
                        EF.Functions.Like(e.Username.ToLower(), $"%{searchTerm}%")
                    );
                }

                // Select only the needed fields, excluding sensitive information
                var employees = await query
                    .Select(e => new
                    {
                        e.Id,
                        e.Nom,
                        e.Prenom,
                        e.Tele,
                        e.Address,
                        e.Username,
                        e.Photo,
                        e.Role,
                        FullName = e.Nom + " " + e.Prenom // Add computed full name
                    })
                    .OrderBy(e => e.Nom)
                    .ToListAsync();

                return Ok(new
                {
                    totalCount = employees.Count,
                    employees = employees
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching employees." });
            }
        }
        // Helper method for creating JWT token
        private string CreateToken(Employees employee)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, employee.Username),
                new Claim(ClaimTypes.Role, employee.Role),
                new Claim("UserId", employee.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}