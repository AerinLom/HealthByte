namespace DietSync.Models
{
    public class Ingredient
    {
        public int IngredientId { get; set; }
        public string Name { get; set; }
        public int Calories { get; set; }
        public float Carbs { get; set; }
        public float Fats { get; set; }
        public float Proteins { get; set; }
        public float Vitamins { get; set; }
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; }
    }
}
