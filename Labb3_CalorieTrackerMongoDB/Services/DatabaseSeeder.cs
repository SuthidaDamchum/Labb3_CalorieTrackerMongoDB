using System.Runtime.CompilerServices;
using System.Windows.Media.Animation;
using Labb3_CalorieTrackerMongoDB.Models;
using MongoDB.Driver;


namespace Labb3_CalorieTrackerMongoDB.Services

{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IMongoDatabase database)
        {
            await SeedFoodsAsync(database);
            await SeedWeeklyGoalsAsync(database);
            await SeedDailyLogsAsync(database);
        }

        private static async Task SeedFoodsAsync(IMongoDatabase database)
        {
            var foodCollection = database.GetCollection<Food>("food");

            if (await foodCollection.Find(_ => true).AnyAsync())
                return;

            var foods = new List<Food>
            {
                new Food { Name = "Egg", Unit = Unit.pcs, Calories = 70, Protein = 6, Carbs = 1, Fat = 5 },
                new Food { Name = "Chicken rice with salad", Unit = Unit.portion, Calories = 420, Protein = 40, Carbs = 45, Fat = 6 },
                new Food { Name = "Apple", Unit = Unit.pcs, Calories = 95, Protein = 0, Carbs = 25, Fat = 0 },
                new Food { Name = "Blueberry whey smoothie", Unit = Unit.portion, Calories = 215, Protein = 34, Carbs = 18, Fat = 2 },
                new Food { Name = "Banana", Unit = Unit.pcs, Calories = 105, Protein = 1, Carbs = 27, Fat = 0 }
            };

            await foodCollection.InsertManyAsync(foods);
        }

        private static async Task SeedWeeklyGoalsAsync(IMongoDatabase database)
        {
            var collection = database.GetCollection<WeeklyGoal>("WeeklyGoals");

            if (await collection.Find(_ => true).AnyAsync())
                return;

            var weeklyGoal = new WeeklyGoal
            {
                WeekStart = StartOfWeek(DateTime.Today, DayOfWeek.Monday),
                GoalCalories = 1450,
                GoalProtein = 105,
                GoalCarbs = 130,
                GoalFat = 47
            };

            await collection.InsertOneAsync(weeklyGoal);
        }

        private static async Task SeedDailyLogsAsync(IMongoDatabase database)
        {
            // MUST match MongoService collection name
            var dailyLogs = database.GetCollection<DailyLog>("dailylogs");
            var foods = database.GetCollection<Food>("food");
            var weeklyGoals = database.GetCollection<WeeklyGoal>("WeeklyGoals");

            if (await dailyLogs.Find(_ => true).AnyAsync())
                return;

            var goal = await weeklyGoals
                .Find(_ => true)
                .SortByDescending(g => g.WeekStart)
                .FirstOrDefaultAsync();

            var smoothie = await foods.Find(f => f.Name == "Blueberry whey smoothie").FirstOrDefaultAsync();
            var chicken = await foods.Find(f => f.Name == "Chicken rice with salad").FirstOrDefaultAsync();
            var apple = await foods.Find(f => f.Name == "Apple").FirstOrDefaultAsync();

            var log = new DailyLog
            {
                Date = DateTime.UtcNow.Date, // keep it simple; consistent UTC date
                GoalCalories = goal?.GoalCalories ?? 1450,
                GoalProtein = (int)(goal?.GoalProtein ?? 105),
                GoalCarbs = (int)(goal?.GoalCarbs ?? 130),
                GoalFat = (int)(goal?.GoalFat ?? 47),
                Items = new List<DailyLogItem>()
            };

            if (smoothie != null) log.Items.Add(CreateItemFromFood(smoothie));
            if (chicken != null) log.Items.Add(CreateItemFromFood(chicken));
            if (apple != null) log.Items.Add(CreateItemFromFood(apple));

            await dailyLogs.InsertOneAsync(log);
        }

        private static DailyLogItem CreateItemFromFood(Food food)
        {
            return new DailyLogItem
            {
                FoodId = food.Id,
                Name = food.Name,

                Unit = food.Unit,
                BaseUnit = food.Unit,
                LogUnit = food.Unit,

                BaseAmount = 1,
                BaseCalories = food.Calories,
                BaseProtein = food.Protein,
                BaseCarbs = food.Carbs,
                BaseFat = food.Fat,

                Amount = 1,
                Calories = food.Calories,
                Protein = food.Protein,
                Carbs = food.Carbs,
                Fat = food.Fat,

                Time = DateTime.Now
            };
        }

        private static DateTime StartOfWeek(DateTime date, DayOfWeek startDay)
        {
            int diff = (7 + (date.DayOfWeek - startDay)) % 7;
            return date.Date.AddDays(-diff);
        }
    }
}