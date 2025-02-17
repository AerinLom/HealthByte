namespace HealthByte.Models
{
    public class Meal
    {
        public int MealId { get; set; } // Primary key
        public int UserId { get; set; } // Foreign key to HealthByteUser
        public DateTime MealDate { get; set; } = DateTime.Now; // Default value of current date
        public string MealName { get; set; } // Name of the meal
        public int? Calories { get; set; } // Calories in the meal
        public decimal? Protein { get; set; } // Protein content in the meal
    }
}
