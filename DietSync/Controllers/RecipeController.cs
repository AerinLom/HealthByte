using DietSync.Data;
using Microsoft.AspNetCore.Mvc;
using DietSync.Models;
using Microsoft.EntityFrameworkCore;


namespace DietSync.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RecipeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Recipe
        [HttpPost]
        public async Task<IActionResult> CreateRecipe([FromBody] RecipeInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Retrieve ingredient details from the database
            var ingredientIds = model.Ingredients.Select(i => i.IngredientId).ToList();
            var ingredients = await _context.Ingredients
                                            .Where(ing => ingredientIds.Contains(ing.IngredientId))
                                            .ToListAsync();

            // Calculate total nutrition
            var totalCalories = 0;
            var totalCarbs = 0f;
            var totalFats = 0f;
            var totalProteins = 0f;
            var totalVitamins = 0f;

            foreach (var recipeIngredient in model.Ingredients)
            {
                var ingredient = ingredients.FirstOrDefault(ing => ing.IngredientId == recipeIngredient.IngredientId);
                if (ingredient != null)
                {
                    // Assuming the quantity in `recipeIngredient.Quantity` is a multiplier for the ingredient's nutritional values
                    var quantityMultiplier = float.Parse(recipeIngredient.Quantity);
                    totalCalories += (int)(ingredient.Calories * quantityMultiplier);
                    totalCarbs += ingredient.Carbs * quantityMultiplier;
                    totalFats += ingredient.Fats * quantityMultiplier;
                    totalProteins += ingredient.Proteins * quantityMultiplier;
                    totalVitamins += ingredient.Vitamins * quantityMultiplier;
                }
            }

            var recipe = new Recipe
            {
                Name = model.Name,
                Description = model.Description,
                TotalCalories = totalCalories,
                TotalCarbs = totalCarbs,
                TotalFats = totalFats,
                TotalProteins = totalProteins,
                TotalVitamins = totalVitamins,
                OtherNutrients = model.OtherNutrients,
                Instructions = model.Instructions,
                CreatedDate = DateTime.UtcNow,
                RecipeIngredients = model.Ingredients.Select(i => new RecipeIngredient
                {
                    IngredientId = i.IngredientId,
                    Quantity = i.Quantity
                }).ToList()
            };

            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            return Ok(recipe);
        }

        [HttpGet("search/{name}")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipeByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Recipe name cannot be empty.");
            }

            var recipes = await _context.Recipes
                .Where(r => r.Name.ToLower().Contains(name.ToLower()))
                .OrderBy(r => r.FoodType)  // Sort by type
                .ToListAsync();

            if (recipes == null || recipes.Count == 0)
            {
                return NotFound("No recipes found with the given name.");
            }

            return Ok(recipes);
        }

        // GET: api/Recipe/type/{type}
        [HttpGet("type/{foodtype}")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipesByFoodType(string foodtype)
        {
            if (string.IsNullOrWhiteSpace(foodtype))
            {
                return BadRequest("Recipe type cannot be empty.");
            }

            var recipes = await _context.Recipes
                .Where(r => r.FoodType.ToLower() == foodtype.ToLower())
                .ToListAsync();

            if (recipes == null || recipes.Count == 0)
            {
                return NotFound("No recipes found for the specified type.");
            }

            return Ok(recipes);
        }

        [HttpGet("type/{healthtype}")]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetRecipesByHealthType(string healthtype)
        {
            if (string.IsNullOrWhiteSpace(healthtype))
            {
                return BadRequest("Recipe type cannot be empty.");
            }

            var recipes = await _context.Recipes
                .Where(r => r.HealthType.ToLower() == healthtype.ToLower())
                .ToListAsync();

            if (recipes == null || recipes.Count == 0)
            {
                return NotFound("No recipes found for the specified type.");
            }

            return Ok(recipes);
        }

        // Input model for creating a recipe
        public class RecipeInputModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int TotalCalories { get; set; }
            public float TotalCarbs { get; set; }
            public float TotalFats { get; set; }
            public float TotalProteins { get; set; }
            public float TotalVitamins { get; set; }
            public string OtherNutrients { get; set; }
            public string Instructions { get; set; }
            public List<RecipeIngredientInputModel> Ingredients { get; set; }
        }

        public class RecipeIngredientInputModel
        {
            public int IngredientId { get; set; }
            public string Quantity { get; set; }
        }

    }
}
