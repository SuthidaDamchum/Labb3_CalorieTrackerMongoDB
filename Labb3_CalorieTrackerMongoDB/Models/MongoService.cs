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

        //public async Task InsertDailyLogAsync(DailyLog dailyLog)
        //{
        //    await DailyLogs.InsertOneAsync(dailyLog);
        //}
        public Task<DailyLog?> GetDailyLogByDateAsync(DateTime date)
        {
            var day = date.Date;
            return DailyLogs.Find(d => d.Date == day).FirstOrDefaultAsync();
        }
        public async Task<DailyLog> GetOrCreateTodayLogAsync(DateTime date)
        {
            var day = date.Date;

            var existing = await DailyLogs
                .Find(d => d.Date == day)

                .FirstOrDefaultAsync();
            if (existing != null)
                return existing;

            var newLog = new DailyLog
            {
                Date = day,
                Items = new List<DailyLogItem>(),
                TotalCalories = 0,
                TotalProtein = 0,
                TotalCarbs = 0,
                TotalFat = 0
            };

            await DailyLogs.InsertOneAsync(newLog);
            return newLog;
        }

        public Task AddItemToDailyLogAsync(ObjectId dailyLogId, DailyLogItem item)
        {
            var filter = Builders<DailyLog>.Filter.Eq(d => d.Id, dailyLogId);

            var update = Builders<DailyLog>.Update
                .Push(d => d.Items, item)
                .Inc(d => d.TotalCalories, item.Calories)
                 .Inc(d => d.TotalProtein, (int)Math.Round(item.Protein))
                .Inc(d => d.TotalCarbs, (int)Math.Round(item.Carbs))
                .Inc(d => d.TotalFat, (int)Math.Round(item.Fat));

            return DailyLogs.UpdateOneAsync(filter, update);
        }

        public async Task RemoveItemFromDailyLogAsync(ObjectId dailyLogId, DailyLogItem item)
        {
            var filter = Builders<DailyLog>.Filter.Eq(log => log.Id, dailyLogId);
            var update = Builders<DailyLog>.Update.PullFilter(
                log => log.Items,
                i => i.FoodId == item.FoodId && i.Time == item.Time
            );
            await DailyLogs.UpdateOneAsync(filter, update);
        }
        public async Task UpdateItemInDailyLogAsync(ObjectId dailyLogId, DailyLogItem item)
        {
            var filter = Builders<DailyLog>.Filter.And(
                Builders<DailyLog>.Filter.Eq(log => log.Id, dailyLogId),
                Builders<DailyLog>.Filter.ElemMatch(
                    log => log.Items,
                    i => i.FoodId == item.FoodId 
                )
            );

            var update = Builders<DailyLog>.Update
                .Set("Items.$.Amount", item.Amount)
                .Set("Items.$.Calories", item.Calories)
                .Set("Items.$.Protein", item.Protein)
                .Set("Items.$.Carbs", item.Carbs)
                .Set("Items.$.Fat", item.Fat)
                .Set("Items.$.Time", item.Time);

            await DailyLogs.UpdateOneAsync(filter, update);
        }
    }
}