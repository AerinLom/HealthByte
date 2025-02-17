using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HealthByteApplication.Models;
using Microsoft.AspNetCore.Identity;
using DietSync.Models;

namespace HealthByteApplication.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7061/api/";

        public UserController()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(UserProfileInputModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Serialize the UserModel object to JSON
                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Send the POST request to the external API
                    var response = await _httpClient.PostAsync("UserProfile/UserProfile", content);

                    // Check for successful response
                    response.EnsureSuccessStatusCode(); // Throws on error status

                    // Redirect to the Login action after successful registration
                    return RedirectToAction("Index", "Home");
                }
                catch (HttpRequestException ex)
                {
                    // Log or handle the exception (e.g., return error view)
                    // Consider logging the exception details for debugging purposes.
                    return View("Error"); // Pass error message to Error view
                }
            }

            // Return validation error or bad request
            return BadRequest(new { success = false, message = "Invalid input" });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var email = model.Email;
                    var response = await _httpClient.GetAsync($"UserProfile/Login/{email}");
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var userProfile = JsonConvert.DeserializeObject<UserProfile>(content);

                    if (userProfile != null)
                    {
                        // Instantiate the PasswordHasher
                        var passwordHasher = new PasswordHasher<UserProfile>();

                        // Compare the entered password with the stored hashed password
                        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(userProfile, userProfile.Password, model.Password);

                        if (passwordVerificationResult == PasswordVerificationResult.Success)
                        {
                            // Login successful
                            TempData["Username"] = userProfile.Username; // Store username in TempData
                            HttpContext.Session.SetString("FirstName", userProfile.FirstName);
                            HttpContext.Session.SetString("Username", userProfile.Username);
                            HttpContext.Session.SetString("UserId", userProfile.UserId.ToString());
                            TempData["SuccessMessage"] = "Login successful! Welcome to HealthByte.";
                            return RedirectToAction("Dashboard", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid username or password.");
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid username or password.");
                        return View(model);
                    }
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError("", "Error while logging in. Please try again later.");
                    return View(); // Return login view with error message
                }
            }

            // If ModelState is not valid, return the login view with validation errors
            ModelState.AddModelError("", "Invalid username or password.");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditSettings()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Home");
            }

            var response = await _httpClient.GetAsync($"UserProfile/GetByUsername/{username}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not load profile data.";
                return RedirectToAction("Dashboard", "Home");
            }

            var content = await response.Content.ReadAsStringAsync();
            var userProfile = JsonConvert.DeserializeObject<UserProfile>(content);

            return View(userProfile);
        }

        [HttpPost]
        public async Task<IActionResult> EditSettings(UserProfile model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var username = HttpContext.Session.GetString("Username");
                    if (string.IsNullOrEmpty(username))
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    var json = JsonConvert.SerializeObject(model);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PutAsync($"UserProfile/Update/{username}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<dynamic>(responseData);

                        TempData["SuccessMessage"] = result.message ?? "Settings updated successfully!";
                        return RedirectToAction("EditSettings");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Failed to update settings.";
                    }
                }
                catch (HttpRequestException)
                {
                    TempData["ErrorMessage"] = "Error updating your settings. Please try again later.";
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Home");
            }

            var response = await _httpClient.GetAsync($"UserProfile/Username/{username}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Could not load profile data.";
                return RedirectToAction("Dashboard", "Home");
            }

            var content = await response.Content.ReadAsStringAsync();
            var userProfile = JsonConvert.DeserializeObject<UserProfile>(content);

            return View(userProfile);
        }

        // ✅ Log Calorie Intake
        [HttpPost]
        public async Task<IActionResult> LogCalories(int calories)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            var logData = new { userId = int.Parse(userId), caloriesConsumed = calories };
            var content = new StringContent(JsonConvert.SerializeObject(logData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("UserProfile/LogMetric", content);
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Calorie intake logged successfully!";
            else
                TempData["ErrorMessage"] = "Failed to log calorie intake.";

            return RedirectToAction("Dashboard", "Home");
        }

        // ✅ Log Water Intake
        [HttpPost]
        public async Task<IActionResult> LogWater(decimal waterAmount)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            var logData = new { userId = int.Parse(userId), waterIntake = waterAmount };
            var content = new StringContent(JsonConvert.SerializeObject(logData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("UserProfile/LogMetric", content);
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Water intake logged successfully!";
            else
                TempData["ErrorMessage"] = "Failed to log water intake.";

            return RedirectToAction("Dashboard", "Home");
        }

        // ✅ Log Workout Time
        [HttpPost]
        public async Task<IActionResult> LogWorkout(int workoutMinutes)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            var logData = new { userId = int.Parse(userId), exerciseMinutes = workoutMinutes };
            var content = new StringContent(JsonConvert.SerializeObject(logData), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("UserProfile/LogMetric", content);
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Workout time logged successfully!";
            else
                TempData["ErrorMessage"] = "Failed to log workout time.";

            return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> LogProteinIntake(decimal proteinGrams)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            var logData = new
            {
                UserId = int.Parse(userId),
                ProteinIntake = proteinGrams
            };

            var json = JsonConvert.SerializeObject(logData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("UserProfile/LogMetric", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Protein intake logged successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to log protein intake.";
            }

            return RedirectToAction("Dashboard", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> LogMeal(Meal meal)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Home");
            }

            // Set the user ID to the meal's UserId
            meal.UserId = int.Parse(userId);

            // Send the meal data to the API
            var json = JsonConvert.SerializeObject(meal);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"UserProfile/CreateMeal/{meal.UserId}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Meal logged successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to log meal.";
            }

            return RedirectToAction("Dashboard", "Home");
        }

        public async Task<IActionResult> MealHistory()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Home");
            }

            var response = await _httpClient.GetAsync($"UserProfile/GetMeals/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var mealsJson = await response.Content.ReadAsStringAsync();
                var meals = JsonConvert.DeserializeObject<List<Meal>>(mealsJson);

                return Json(meals);
            }
            else
            {
                return Json(new List<Meal>()); // Return an empty list if the request fails
            }
        }




        public async Task<IActionResult> Logout()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
