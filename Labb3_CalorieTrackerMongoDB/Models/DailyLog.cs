using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Labb3_CalorieTrackerMongoDB.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Labb3_CalorieTrackerMongoDB.Models
{
    public class DailyLog
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public DateTime Date { get; set; }
        public int TotalCalories { get; set; }
        public int TotalProtein { get; set; }
        public int TotalCarbs { get; set; }
        public int TotalFat { get; set; }
        public List<Food> Foods { get; set; } 
    }

}


