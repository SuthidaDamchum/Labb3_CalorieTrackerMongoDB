using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Labb3_CalorieTrackerMongoDB.Models
{
    public class Food
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Name { get; set; }

        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }

        public int Fat { get; set; }

    }

}