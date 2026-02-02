using Labb3_CalorieTrackerMongoDB.Models;
using MongoDB.Driver;


namespace Labb3_CalorieTrackerMongoDB.Services

{
    public class DatabaseSeeder
    {
        private readonly MongoService _mongoService;

        public DatabaseSeeder(MongoService mongoService) => this._mongoService = mongoService;
        public async Task SeedAsync()
        {
            await SeedFoodsAsync();
            await SeedWeeklyGoalsAsync();
            await SeedDailyLogsAsync();
        }

        private async Task SeedFoodsAsync()
        {
            var foodCollection = _mongoService.Foods;

            if (await foodCollection.Find(_ => true).AnyAsync())
                return;

            var foods = new List<Food>
            {
                new Food { Name = "Egg", Unit = Unit.pcs, Calories = 70, Protein = 6, Carbs = 1, Fat = 5 },
                new Food { Name = "Chicken rice with salad", Unit = Unit.portion, Calories = 420, Protein = 40, Carbs = 45, Fat = 6 },
                new Food { Name = "Apple", Unit = Unit.pcs, Calories = 95, Protein = 0, Carbs = 25, Fat = 0 },
                new Food { Name = "Blueberry whey smoothie", Unit = Unit.portion, Calories = 215, Protein = 34, Carbs = 18, Fat = 2 },
                new Food { Name = "Banana", Unit = Unit.pcs, Calories = 105, Protein = 1, Carbs = 27, Fat = 0 },
                new Food { Name = "Milk", Unit = Unit.dl, Calories = 60, Protein = 3.3, Carbs = 4.8, Fat = 3.3 }
            };

            await foodCollection.InsertManyAsync(foods);
        }

        private async Task SeedWeeklyGoalsAsync()
        {
            var collection = _mongoService.WeeklyGoals;

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

        private async Task SeedDailyLogsAsync()
        {
            
            var dailyLogs = _mongoService.DailyLogs;
            var foods = _mongoService.Foods;
            var weeklyGoals = _mongoService.WeeklyGoals;

            if (await dailyLogs.Find(_ => true).AnyAsync())
                return;

            var goal = await weeklyGoals
                .Find(_ => true)
                .SortByDescending(g => g.WeekStart)
                .FirstOrDefaultAsync();

            var smoothie = await foods.Find(f => f.Name == "Blueberry whey smoothie").FirstOrDefaultAsync();
            var chicken = await foods.Find(f => f.Name == "Chicken rice with salad").FirstOrDefaultAsync();
            var apple = await foods.Find(f => f.Name == "Apple").FirstOrDefaultAsync();
            var milk = await foods.Find(f => f.Name == "Milk").FirstOrDefaultAsync(); 

            var log = new DailyLog
            {
                Date = DateTime.UtcNow.Date, 
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