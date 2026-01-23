using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Labb3_CalorieTrackerMongoDB.Models
{
    public class Food
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; } = "";
        public double Amount { get; set; } = 1;

        [BsonRepresentation(BsonType.String)]
        public Unit Unit { get; set; } = Unit.g;


        public int Calories { get; set; }
        public int Protein { get; set; }
        public int Carbs { get; set; }
        public int Fat { get; set; }
       


        }
    }
