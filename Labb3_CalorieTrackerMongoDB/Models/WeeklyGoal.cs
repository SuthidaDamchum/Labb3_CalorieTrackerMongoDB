using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Labb3_CalorieTrackerMongoDB.Models
{
    public class WeeklyGoal
    {
        [BsonId]
        public ObjectId Id { get; set; }


        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime WeekStart { get; set; }

        public int GoalCalories { get; set; }
        public double GoalProtein { get; set; }
        public double GoalCarbs { get; set; }
        public double GoalFat { get; set; }
    }
}
