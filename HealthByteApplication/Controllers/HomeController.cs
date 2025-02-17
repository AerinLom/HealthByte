using HealthByteApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HealthByteApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7061/api/";

        public HomeController()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "User");

            decimal waterIntake = 0.0m;  // Default value
            decimal proteinIntake = 0.0m; // Default value
            int exerciseMin = 0;
            decimal totalCalories = 0.0m; // Default value for total calories

            try
            {
                // ? Make both API calls asynchronously for water and exercise
                var responseWater = await _httpClient.GetAsync($"UserProfile/GetDailyWater/{userId}");
                var responseExercise = await _httpClient.GetAsync($"UserProfile/GetDailyExercise/{userId}");
                var responseCalories = await _httpClient.GetAsync($"UserProfile/GetTotalCalories/{userId}");  // API for total calories
                var responseProtein = await _httpClient.GetAsync($"UserProfile/GetTotalProtein/{userId}");    // API for total protein

                // ? Process Water Intake response
                if (responseWater.IsSuccessStatusCode)
                {
                    var contentWater = await responseWater.Content.ReadAsStringAsync();
                    var resultWater = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(contentWater);

                    if (resultWater != null && resultWater.ContainsKey("waterIntake"))
                    {
                        waterIntake = resultWater["waterIntake"];
                    }
                }

                // ? Process Exercise Minutes response
                if (responseExercise.IsSuccessStatusCode)
                {
                    var contentExercise = await responseExercise.Content.ReadAsStringAsync();
                    var resultExercise = JsonConvert.DeserializeObject<Dictionary<string, int>>(contentExercise);
                    if (resultExercise.ContainsKey("exerciseMin"))
                        exerciseMin = resultExercise["exerciseMin"];
                }

                // ? Process Total Calories response
                if (responseCalories.IsSuccessStatusCode)
                {
                    var contentCalories = await responseCalories.Content.ReadAsStringAsync();
                    var resultCalories = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(contentCalories);
                    if (resultCalories != null && resultCalories.ContainsKey("totalCalories"))
                    {
                        totalCalories = resultCalories["totalCalories"];
                    }
                }

                // ? Process Total Protein response
                if (responseProtein.IsSuccessStatusCode)
                {
                    var contentProtein = await responseProtein.Content.ReadAsStringAsync();
                    var resultProtein = JsonConvert.DeserializeObject<Dictionary<string, decimal>>(contentProtein);
                    if (resultProtein != null && resultProtein.ContainsKey("totalProtein"))
                    {
                        proteinIntake = resultProtein["totalProtein"];
                    }
                }
            }
            catch (Exception ex)
            {
                // ? Log the error (optional)
                Console.WriteLine($"Error fetching daily logs: {ex.Message}");
            }

            // ? Pass the values to the View
            ViewBag.WaterIntake = waterIntake;
            ViewBag.ExerciseMin = exerciseMin;
            ViewBag.TotalCalories = totalCalories;
            ViewBag.ProteinIntake = proteinIntake;

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
