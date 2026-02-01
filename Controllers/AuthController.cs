using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HseBackend.Data;
using HseBackend.Models;
using System.Threading.Tasks;

namespace HseBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest("Email already exists.");
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully", user = user });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            // Return user info (excluding password ideally, but here for simplicity)
            return Ok(new { 
                message = "Login successful", 
                userId = user.Id,
                fullName = user.FullName,
                role = user.Role, 
                email = user.Email,
                jobNumber = user.JobNumber,
                mobile = user.Mobile,
                department = user.Department
            });
        }

        // --- NEW: User Management for Supervisor ---
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.OrderBy(u => u.FullName).ToListAsync();
        }

        [HttpPut("update-role")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return NotFound("User not found");

            user.Role = request.Role;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"User role updated to {request.Role}" });
        }
        // --- NEW: Profile Management ---
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile([FromQuery] string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.FullName,
                user.Email,
                user.JobNumber,
                user.Mobile,
                user.Department,
                user.Role,
                user.ProfilePicture
            });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return NotFound("User not found");

            user.FullName = request.FullName;
            user.Mobile = request.PhoneNumber;
            user.JobNumber = request.JobId;
            user.Department = request.Department;
            
            if (!string.IsNullOrEmpty(request.ProfilePicture))
            {
                user.ProfilePicture = request.ProfilePicture;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully" });
        }
    }

    public class UpdateRoleRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string JobId { get; set; }
        public string Department { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
