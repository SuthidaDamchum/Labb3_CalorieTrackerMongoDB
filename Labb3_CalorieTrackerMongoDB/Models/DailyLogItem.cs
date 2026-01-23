using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Labb3_CalorieTrackerMongoDB.Models
{
    public class DailyLogItem
    {
        public ObjectId FoodId { get; set; }
        public string Name { get; set; } = "";

        public double Amount { get; set; }  // e.g. 2, 150, 250

        [BsonRepresentation(BsonType.String)]
        public Unit Unit { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        // Calculated totals for this eaten item (so WeeklySummary is easy)
        public int Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }

    }
}
