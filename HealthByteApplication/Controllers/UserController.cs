using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HealthByteApplication.Models;
using Microsoft.AspNetCore.Identity;

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
                            HttpContext.Session.SetString("Username", userProfile.Username);
                            HttpContext.Session.SetString("UserId", userProfile.UserId.ToString());
                            TempData["SuccessMessage"] = "Login successful! Welcome to HealthByte.";
                            return RedirectToAction("Dashboard", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Invalid username or password.");
                            return View(model); // Return login view with error message
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




        public async Task<IActionResult> Logout()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
