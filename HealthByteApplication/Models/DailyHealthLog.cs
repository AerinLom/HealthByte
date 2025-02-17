namespace HealthByteApplication.Models
{
    public class DailyHealthLog
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public DateTime LogDate { get; set; } = DateTime.UtcNow;
        public int? CaloriesConsumed { get; set; }
        public decimal? WaterIntake { get; set; }
        public decimal? ProteinIntake { get; set; }
        public int? ExerciseMinutes { get; set; }
        public int? StepsTaken { get; set; }
        public string Notes { get; set; }

        // Navigation Property (Optional)
        public UserProfile User { get; set; }
    }

}
