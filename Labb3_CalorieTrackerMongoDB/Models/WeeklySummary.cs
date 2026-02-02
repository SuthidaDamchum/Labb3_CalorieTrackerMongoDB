using Labb3_CalorieTrackerMongoDB.ViewModels;

namespace Labb3_CalorieTrackerMongoDB.Models
{
    public class WeeklySummary 
    { 
        public DateTime Date { get; set; }
        public bool HasLog { get; set; }

        public int GoalCalories { get; set; }
        public double GoalProtein { get; set; }
        public double GoalCarbs { get; set; }
        public double GoalFat { get; set; }

        public int ActualCalories { get; set; }
        public double ActualProtein { get; set; }
        public double ActualCarbs { get; set; }
        public double ActualFat { get; set; }

        public int CaloriesDiff => HasLog ? ActualCalories - GoalCalories : 0;

        public string CaloriesDiffText =>
            !HasLog ? "—" :
            CaloriesDiff > 0 ? $"+{CaloriesDiff}" :
            CaloriesDiff.ToString();

        public string ProteinText => !HasLog ? "—" : $"{ActualProtein}/{GoalProtein}";
        public string CarbsText => !HasLog ? "—" : $"{ActualCarbs}/{GoalCarbs}";
        public string FatText => !HasLog ? "—" : $"{ActualFat}/{GoalFat}";
    }
}
