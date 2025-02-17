using Microsoft.AspNetCore.Mvc;
using DietSync.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DietSync.Data;
using Microsoft.AspNetCore.Identity;
using HealthByte.Models;

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

        // GET: api/UserProfile/Username/{username}
        [HttpGet("Login/{email}")]
        public async Task<ActionResult<UserProfile>> GetUserProfileByEmail(string email)
        {
            try
            {
                // Retrieve a user profile by username from the database
                var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Email == email);

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
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateofBirth = null, // Optional fields not included in signup
                    Gender = null,
                    Weight = null,
                    Height = null,
                    Membership = "Free" // Default membership level
                };

                // Hash the password
                userProfile.Password = _passwordHasher.HashPassword(userProfile, model.Password);

                // Save user profile
                _context.UserProfiles.Add(userProfile);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while saving the user profile.");
            }
        }

        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] SignInModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Find the user by username or email
                var user = await _context.UserProfiles
                    .FirstOrDefaultAsync(u => u.Username == model.UsernameOrEmail || u.Email == model.UsernameOrEmail);

                if (user == null)
                {
                    return Unauthorized(new { message = "Invalid username or password." });
                }

                // Verify the password
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

                if (passwordVerificationResult == PasswordVerificationResult.Failed)
                {
                    return Unauthorized(new { message = "Invalid username or password." });
                }

                // Authentication successful
                return Ok(new { message = "Login successful", userId = user.UserId, username = user.Username });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred during sign-in.");
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

        [HttpPut("Update/{username}")]
        public async Task<IActionResult> UpdateSettings(string username, [FromBody] UserProfile updatedProfile)
        {
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.Username == username);
            if (userProfile == null)
            {
                return NotFound("User not found.");
            }

            // Update fields
            userProfile.Username = updatedProfile.Username ?? userProfile.Username;
            userProfile.DateofBirth = updatedProfile.DateofBirth ?? userProfile.DateofBirth;
            userProfile.Gender = updatedProfile.Gender ?? userProfile.Gender;
            userProfile.Weight = updatedProfile.Weight ?? userProfile.Weight;
            userProfile.Height = updatedProfile.Height ?? userProfile.Height;

            // Save changes
            await _context.SaveChangesAsync();

            return Ok(userProfile);
        }

        [HttpPost("LogMetric")]
        public async Task<IActionResult> LogMetric([FromBody] DailyHealthLog logEntry)
        {
            if (logEntry == null || logEntry.UserId <= 0)
            {
                return BadRequest("Invalid input: UserId is required.");
            }

            var today = DateTime.UtcNow.Date;
            var existingLog = await _context.DailyHealthLogs
                .FirstOrDefaultAsync(log => log.UserId == logEntry.UserId && log.LogDate == today);

            if (existingLog != null)
            {
                // ✅ Accumulate values instead of overwriting
                if (logEntry.CaloriesConsumed.HasValue)
                    existingLog.CaloriesConsumed = (existingLog.CaloriesConsumed ?? 0) + logEntry.CaloriesConsumed;

                if (logEntry.WaterIntake.HasValue)
                    existingLog.WaterIntake = (existingLog.WaterIntake ?? 0) + logEntry.WaterIntake;

                if (logEntry.ProteinIntake.HasValue)
                    existingLog.ProteinIntake = (existingLog.ProteinIntake ?? 0) + logEntry.ProteinIntake;

                if (logEntry.ExerciseMinutes.HasValue)
                    existingLog.ExerciseMinutes = (existingLog.ExerciseMinutes ?? 0) + logEntry.ExerciseMinutes;

                if (logEntry.StepsTaken.HasValue)
                    existingLog.StepsTaken = (existingLog.StepsTaken ?? 0) + logEntry.StepsTaken;

                if (!string.IsNullOrEmpty(logEntry.Notes))
                    existingLog.Notes = logEntry.Notes;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Daily log updated successfully." });
            }
            else
            {
                logEntry.LogDate = today;
                _context.DailyHealthLogs.Add(logEntry);
                await _context.SaveChangesAsync();
                return Ok(new { message = "New daily log created successfully." });
            }
        }

        [HttpGet("GetDailyWater/{userId}")]
        public async Task<IActionResult> GetDailyWater(int userId)
        {
            var today = DateTime.UtcNow.Date;

            var dailyLog = await _context.DailyHealthLogs
                .FirstOrDefaultAsync(log => log.UserId == userId && log.LogDate == today);

            if (dailyLog == null)
            {
                return NotFound(new { message = "No water intake log found for today." });
            }

            return Ok(new { waterIntake = dailyLog.WaterIntake });
        }

        [HttpGet("GetDailyProtein/{userId}")]
        public async Task<IActionResult> GetDailyProtein(int userId)
        {
            var today = DateTime.UtcNow.Date;

            var dailyLog = await _context.DailyHealthLogs
                .FirstOrDefaultAsync(log => log.UserId == userId && log.LogDate == today);

            if (dailyLog == null)
            {
                return NotFound(new { message = "No water intake log found for today." });
            }

            return Ok(new { proteinIntake = dailyLog.ProteinIntake });
        }

        [HttpGet("GetDailyExercise/{userId}")]
        public async Task<IActionResult> GetDailyExercise(int userId)
        {
            var today = DateTime.UtcNow.Date;

            var dailyLog = await _context.DailyHealthLogs
                .FirstOrDefaultAsync(log => log.UserId == userId && log.LogDate == today);

            if (dailyLog == null)
            {
                return NotFound(new { message = "No water intake log found for today." });
            }

            return Ok(new { exerciseMin = dailyLog.ExerciseMinutes });
        }



        [HttpGet("GetDailyLogs/{userId}")]
        public async Task<IActionResult> GetDailyLogs(int userId)
        {
            var logs = await _context.DailyHealthLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.LogDate)
                .ToListAsync();

            return Ok(logs);
        }

        [HttpGet("GetDailyLog/{userId}")]
        public async Task<IActionResult> GetDailyLog(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var today = DateTime.UtcNow.Date;
            var dailyLog = await _context.DailyHealthLogs
                .FirstOrDefaultAsync(log => log.UserId == userId && log.LogDate == today);

            if (dailyLog == null)
            {
                return NotFound(new { message = "No log found for today." });
            }

            return Ok(dailyLog);
        }

        [HttpPost("CreateMeal/{userId}")]
        public async Task<IActionResult> CreateMeal(int userId, [FromBody] Meal meal)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            if (meal == null)
            {
                return BadRequest("Meal data is required.");
            }

            // Optionally, validate meal properties (e.g., ensure that MealName, Calories, etc. are provided)
            if (string.IsNullOrEmpty(meal.MealName) || meal.Calories == null || meal.Protein == null)
            {
                return BadRequest("Meal name, calories, and protein are required.");
            }

            meal.UserId = userId; // Assign the UserId to the meal
            meal.MealDate = DateTime.UtcNow.Date; // Set today's date for the meal

            // Add the meal to the context
            _context.Meals.Add(meal);

            // Save changes to the database
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMealsForUserToday), new { userId = userId }, meal);
        }



        [HttpGet("GetMeals/{userId}")]
        public async Task<IActionResult> GetMealsForUserToday(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var today = DateTime.UtcNow.Date;
            var meals = await _context.Meals
                                      .Where(m => m.UserId == userId && m.MealDate.Date == today)
                                      .ToListAsync();

            if (meals == null || meals.Count == 0)
            {
                return NotFound(new { message = "No meals found for this user today." });
            }

            return Ok(meals);
        }

        [HttpGet("GetTotalCalories/{userId}")]
        public async Task<IActionResult> GetTotalCaloriesForUserToday(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var today = DateTime.UtcNow.Date;
            var totalCalories = await _context.Meals
                                               .Where(m => m.UserId == userId && m.MealDate.Date == today)
                                               .SumAsync(m => m.Calories ?? 0);

            return Ok(new { TotalCalories = totalCalories });
        }

        [HttpGet("GetTotalProtein/{userId}")]
        public async Task<IActionResult> GetTotalProteinForUserToday(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var today = DateTime.UtcNow.Date;
            var totalProtein = await _context.Meals
                                              .Where(m => m.UserId == userId && m.MealDate.Date == today)
                                              .SumAsync(m => m.Protein ?? 0);

            return Ok(new { TotalProtein = totalProtein });
        }


        // Check if a user profile with specified id exists
        private bool UserProfileExists(int id)
        {
            return _context.UserProfiles.Any(e => e.UserId == id);
        }
    }
}