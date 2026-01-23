using System.Threading.Tasks;
using Labb3_CalorieTrackerMongoDB.Models;
using MongoDB.Bson;
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



        //public async Task MigrateFoodsAsync(MongoService mongo)
        //{
        //    // Add defaults only to documents that don't have Unit yet
        //    var filter = Builders<Food>.Filter.Exists("Unit", false);

        //    var update = Builders<Food>.Update
        //        .Set("Unit", Unit.g.ToString())
        //        .Set("Amount", 100)


        //    await mongo.Foods.UpdateManyAsync(filter, update);
        //}

        public async Task InsertDailyLogAsync(DailyLog dailyLog)
        {
            await DailyLogs.InsertOneAsync(dailyLog);
        }

        public async Task AddItemToDailyLogAsync(ObjectId dailyLogId, DailyLogItem item)
        {
            var filter = Builders<DailyLog>.Filter.Eq(d => d.Id, dailyLogId);
            var update = Builders<DailyLog>.Update.Push(d => d.Items, item);
            await DailyLogs.UpdateOneAsync(filter, update);
        }
    }

}







