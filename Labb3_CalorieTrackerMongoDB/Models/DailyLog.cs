using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Linq;

namespace Labb3_CalorieTrackerMongoDB.Models
{
    public class DailyLog
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public DateTime Date { get; set; }
        public List<DailyLogItem> Items { get; set; } = new();
    
        public int GoalCalories { get; set; }
        public int GoalProtein { get; set; }
        public int GoalCarbs { get; set; }
        public int GoalFat { get; set; }

        [BsonIgnore]
        public int TotalCalories => Items?.Sum(i => i.Calories) ?? 0;
    }
}



