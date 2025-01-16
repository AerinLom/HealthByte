namespace DietSync.Models
{
    public class Recipe
    {
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int TotalCalories { get; set; }
        public float TotalCarbs { get; set; }
        public float TotalFats { get; set; }
        public float TotalProteins { get; set; }
        public float TotalVitamins { get; set; }
        public string OtherNutrients { get; set; }
        public string Instructions { get; set; }
        public DateTime CreatedDate { get; set; }
        public String? Type { get; set; }
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; }
    }
}
