using Labb3_CalorieTrackerMongoDB.Models;
using MongoDB.Driver;

namespace Labb3_CalorieTrackerMongoDB.Models
{

    public class MongoService
    {
        private readonly IMongoDatabase _database;

        public MongoService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            _database = client.GetDatabase("SuthidaDamchum"); // Din databas
        }
        public IMongoCollection<Food> Foods => _database.GetCollection<Food>("food");
        public IMongoCollection<DailyLog> DailyLogs => _database.GetCollection<DailyLog>("dailylogs");
    }
}


