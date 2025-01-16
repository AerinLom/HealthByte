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
