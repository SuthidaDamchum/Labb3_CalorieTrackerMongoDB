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
            _database = client.GetDatabase("SuthidaDamchum"); 
        }
        public IMongoCollection<Food> Foods => _database.GetCollection<Food>("food");
        public IMongoCollection<DailyLog> DailyLogs => _database.GetCollection<DailyLog>("dailylogs");


        public TimeZoneInfo StockholmTz => GetStockholmTimeZone();

        public DateTime GetWeekStartUtcStockholm(DateTime weekStartLocalDate)
        {

            var tz = StockholmTz;
            var unspecified = DateTime.SpecifyKind(weekStartLocalDate.Date, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(unspecified, tz);
        }


        private static TimeZoneInfo GetStockholmTimeZone()
        {
            try
            {
    
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");
            }
            catch (TimeZoneNotFoundException)
            {
          
                return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            }
        }

        private static DateTime ToUtcStartOfDayStockholm(DateTime localDate)
        {
            var tz = GetStockholmTimeZone();
            var unspecified = DateTime.SpecifyKind(localDate.Date, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(unspecified, tz);
        }

        public Task UpdateDailyGoalsAyncs(DateTime localDate, int kcal, int p, int c, int f)
        {
            var startUtc = ToUtcStartOfDayStockholm(localDate);
            var endUtc = startUtc.AddDays(1);

            var filter = Builders<DailyLog>.Filter.And(
                Builders<DailyLog>.Filter.Gte(x => x.Date, startUtc),
                Builders<DailyLog>.Filter.Lt(x => x.Date, endUtc)
            );

            var update = Builders<DailyLog>.Update
                .Set(x => x.GoalCalories, kcal)
                .Set(x => x.GoalProtein, p)
                .Set(x => x.GoalCarbs, c)
                .Set(x => x.GoalFat, f);

            return DailyLogs.UpdateOneAsync(filter, update);
        }


        public async Task<DailyLog?> GetDailyLogByDateAsync(DateTime localDate)
        {
            var startUtc = ToUtcStartOfDayStockholm(localDate);
            var endUtc = startUtc.AddDays(1);

            return await DailyLogs
                .Find(x => x.Date >= startUtc && x.Date < endUtc)
                .FirstOrDefaultAsync();
        }



        public Task AddItemToDailyLogAsync(ObjectId dailyLogId, DailyLogItem item)
        {
            var filter = Builders<DailyLog>.Filter.And(
                Builders<DailyLog>.Filter.Eq(d => d.Id, dailyLogId),
                Builders<DailyLog>.Filter.Not(
                    Builders<DailyLog>.Filter.ElemMatch(d => d.Items, i => i.FoodId == item.FoodId)
                )
            );

            var update = Builders<DailyLog>.Update.Push(d => d.Items, item);
            return DailyLogs.UpdateOneAsync(filter, update);
        }


        public async Task RemoveItemFromDailyLogAsync(ObjectId dailyLogId, ObjectId foodID)
        {
            var filter = Builders<DailyLog>.Filter.Eq(log => log.Id, dailyLogId);
            var update = Builders<DailyLog>.Update.PullFilter(
                log => log.Items,
                i => i.FoodId == foodID
            );
            await DailyLogs.UpdateOneAsync(filter, update);
        }
        public async Task UpdateItemInDailyLogAsync(ObjectId dailyLogId, DailyLogItem item)
        {
            var filter = Builders<DailyLog>.Filter.And(
            Builders<DailyLog>.Filter.Eq(log => log.Id, dailyLogId),
            Builders<DailyLog>.Filter.ElemMatch(log => log.Items, i => i.FoodId == item.FoodId)
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

        public async Task<DailyLog> GetOrCreateTodayLogAsync(DateTime localDate)
        {
            var startUtc = ToUtcStartOfDayStockholm(localDate);
            var endUtc = startUtc.AddDays(1);

            var existing = await DailyLogs
                .Find(x => x.Date >= startUtc && x.Date < endUtc)
                .FirstOrDefaultAsync();

            if (existing != null)
                return existing;

            var newLog = new DailyLog
            {
                Date = startUtc,
                Items = new List<DailyLogItem>()
            };

            await DailyLogs.InsertOneAsync(newLog);
            return newLog;
        }

    }
}