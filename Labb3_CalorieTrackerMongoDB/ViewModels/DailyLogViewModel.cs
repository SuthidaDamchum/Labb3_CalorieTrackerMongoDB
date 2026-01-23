using System.Collections.ObjectModel;
using System.Runtime.Versioning;
using System.Windows.Input;
using Labb3_CalorieTrackerMongoDB.Commands;
using Labb3_CalorieTrackerMongoDB.Models;
using MongoDB.Bson;

namespace Labb3_CalorieTrackerMongoDB.ViewModels
{
    public class DailyLogViewModel : ViewModelBase
    {
        public ObservableCollection<DailyLogItem> TodaysItems { get; } = new();

        private readonly MongoService _mongoService;
        public ICommand SaveDayCommand { get; }
        public DateTime TodayDate => DateTime.Today;
        public int GoalCalories => 1500;
        public int GoalProtein => 100;
        public int GoalCarbs => 150;
        public int GoalFat => 50;


        public int TotalCalories => TodaysItems.Sum(f => f.Calories);
        public int TotalProtein => (int)Math.Round(TodaysItems.Sum(f => f.Protein));
        public int TotalCarbs => (int)Math.Round(TodaysItems.Sum(f => f.Carbs));
        public int TotalFat => (int)Math.Round(TodaysItems.Sum(f => f.Fat));

        public DailyLogViewModel(MongoService mongoService)
        {
            _mongoService = mongoService;

            SaveDayCommand = new AsyncDelegateCommand(
                _ => SaveDayAsync(), _ => TodaysItems.Any());


            TodaysItems.CollectionChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(TotalCalories));
                RaisePropertyChanged(nameof(TotalProtein));
                RaisePropertyChanged(nameof(TotalCarbs));
                RaisePropertyChanged(nameof(TotalFat));
            };
        }
        
        private async Task SaveDayAsync()
        {
            if (!TodaysItems.Any())
                return;

            var dailyLog = new DailyLog
            {
                Date = DateTime.Today,
                Items = TodaysItems.ToList(),
                TotalCalories = TotalCalories,
                TotalProtein = TotalProtein,
                TotalCarbs = TotalCarbs,
                TotalFat = TotalFat
            };

            await _mongoService.InsertDailyLogAsync(dailyLog);

        }

        public void AddFoodFromList(Food f, double ConsumedAmount)
        {

            var factor = ConsumedAmount / f.Amount;

            var item = new DailyLogItem
            {
                FoodId = f.Id,
                Name = f.Name,

                Unit = f.Unit,
                Amount = ConsumedAmount,
                Calories = (int)Math.Round(factor * f.Calories),
                Protein = factor * f.Protein,
                Carbs = factor * f.Carbs,
                Fat = factor * f.Fat,

                Time = DateTime.Now
            };

            TodaysItems.Add(item);

            // Update totals UI
            RaisePropertyChanged(nameof(TotalCalories));
            RaisePropertyChanged(nameof(TotalProtein));
            RaisePropertyChanged(nameof(TotalCarbs));
            RaisePropertyChanged(nameof(TotalFat));

            // Enable Save button
            (SaveDayCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
        }

        public async Task AddFoodToExistingDailyLogAsync(ObjectId dailyLogId, Food food, double amount)
        {
            var factor = amount / food.Amount;
            var newItem = new DailyLogItem
            {
                FoodId = food.Id,
                Name = food.Name,
                Amount = amount,
                Unit = food.Unit,
                Time = DateTime.Now,
                Calories = (int)Math.Round(factor * food.Calories),
                Protein = factor * food.Protein,
                Carbs = factor * food.Carbs,
                Fat = factor * food.Fat
            };

            await _mongoService.AddItemToDailyLogAsync(dailyLogId, newItem);
        }
    }

}

































































