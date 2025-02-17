using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DietSync.Models;
public class DailyHealthLog
{
    [Key] // Ensure LogId is a primary key
    public int LogId { get; set; }

    [Required] // UserId must always be provided
    public int UserId { get; set; }

    [Column(TypeName = "DATE")]
    public DateTime LogDate { get; set; }
    public int? CaloriesConsumed { get; set; }
    public decimal? WaterIntake { get; set; }
    public decimal? ProteinIntake { get; set; }
    public int? ExerciseMinutes { get; set; }
    public int? StepsTaken { get; set; }
    public string? Notes { get; set; }

    // Navigation Property
    public UserProfile? User { get; set; }
}
