using Microsoft.AspNetCore.Mvc;
using DietSync.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DietSync.Data;
using Microsoft.AspNetCore.Identity;

namespace DietSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<UserProfile> _passwordHasher;

        public UserProfileController(ApplicationDbContext context, IPasswordHasher<UserProfile> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserProfile>>> GetUserProfiles()
        {
            // Retrieve all user profiles from the database
            return await _context.UserProfiles.ToListAsync();
        }

        // GET: api/UserProfile/Id/{userId}
        [HttpGet("Id/{userId}")]
        public async Task<ActionResult<UserProfile>> GetUserProfileByID(int userId)
        {
            try
            {
                // Retrieve a user profile by userId from the database
                var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.UserId == userId);

                if (userProfile == null)
                {
                    // Return 404 Not Found if user profile with specified userId is not found
                    return NotFound();
                }

                // Return the user profile if found
                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                // Log and return 500 Internal Server Error for any exceptions
                // Example: _logger.LogError(ex, $"Error occurred while fetching UserProfile with userId {userId}");
                return StatusCode(500, "An error occurred while fetching the user profile.");
            }
        }

        // GET: api/UserProfile/Username/{username}
        [HttpGet("Username/{username}")]
        public async Task<ActionResult<UserProfile>> GetUserProfileByUsername(string username)
        {
            try
            {
                // Retrieve a user profile by username from the database
                var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Username == username);

                if (userProfile == null)
                {
                    // Return 404 Not Found if user profile with specified username is not found
                    return NotFound();
                }

                // Return the user profile if found
                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                // Log and return 500 Internal Server Error for any exceptions
                // Example: _logger.LogError(ex, $"Error occurred while fetching UserProfile with username {username}");
                return StatusCode(500, "An error occurred while fetching the user profile.");
            }
        }

        [HttpPost("UserProfile")]
        public async Task<IActionResult> PostUserProfile([FromBody] UserProfileInputModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userProfile = new UserProfile
                {
                    Username = model.Username,
                    Email = model.Email
                    // Add other properties as needed
                };

                // Hash the password
                userProfile.Password = _passwordHasher.HashPassword(userProfile, model.Password);

                // Add the new user profile to the database
                _context.UserProfiles.Add(userProfile);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while saving the user profile.");
            }
        }

        // PUT: api/UserProfile/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserProfile(int id, UserProfileInputModel updatedUserProfile)
        {
            var userProfile = await _context.UserProfiles.FindAsync(id);

            if (userProfile == null)
            {
                return NotFound("UserProfile not found.");
            }

            if (updatedUserProfile.Username != null)
            {
                userProfile.Username = updatedUserProfile.Username;
            }
            if (updatedUserProfile.Password != null)
            {
                // Hash the new password
                userProfile.Password = _passwordHasher.HashPassword(userProfile, updatedUserProfile.Password);
            }
            if (updatedUserProfile.Email != null)
            {
                userProfile.Email = updatedUserProfile.Email;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserProfileExists(id))
                {
                    return NotFound("UserProfile not found.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserProfile(int id)
        {
            var userProfile = await _context.UserProfiles.FindAsync(id);
            if (userProfile == null)
            {
                // Return 404 Not Found if user profile with specified id is not found
                return NotFound();
            }

            // Remove user profile from the database
            _context.UserProfiles.Remove(userProfile);
            await _context.SaveChangesAsync();

            // Return HTTP 204 No Content after successful deletion
            return NoContent();
        }

        // Check if a user profile with specified id exists
        private bool UserProfileExists(int id)
        {
            return _context.UserProfiles.Any(e => e.UserId == id);
        }
    }
}