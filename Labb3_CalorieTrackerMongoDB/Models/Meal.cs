using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Labb3_CalorieTrackerMongoDB.Models
{
    public class Meal
    {
        [BsonRepresentation(BsonType.String)]
        public MealType MealType { get; set; }
        public List<Food> Foods { get; set; } = new List<Food>();
    }

    public enum MealType
    {
        Breakfast,
        Lunch,
        Dinner,
        Sanck
    }
}

