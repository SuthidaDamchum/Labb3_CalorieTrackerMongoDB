using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labb3_CalorieTrackerMongoDB.Models
{
    public class DailyLogSummary
    {
        public DateTime Date { get; set; }

        public int GoalCalories { get; set; } = 1500;
        public int ActualCalories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }

        public string Result => ActualCalories <= GoalCalories ? "✅" : "⚠️";
    }
}