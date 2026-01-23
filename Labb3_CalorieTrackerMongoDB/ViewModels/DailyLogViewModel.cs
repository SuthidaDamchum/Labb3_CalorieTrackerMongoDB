using System.Collections.ObjectModel;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Labb3_CalorieTrackerMongoDB.Commands;
using Labb3_CalorieTrackerMongoDB.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Labb3_CalorieTrackerMongoDB.ViewModels
{
    public class DailyLogViewModel : ViewModelBase
    {
        public ObservableCollection<DailyLogItem> TodaysItems { get; } = new();

        private readonly MongoService _mongoService;

        private ObjectId _todayLogId = ObjectId.Empty;

        public DateTime TodayDate => DateTime.Today; 
        public ICommand DeleteItemCommand { get; }

        private DailyLogItem? _selectedItem;
        public DailyLogItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                RaisePropertyChanged();
                (DeleteItemCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
         
            }
        }

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


            TodaysItems.CollectionChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(TotalCalories));
                RaisePropertyChanged(nameof(TotalProtein));
                RaisePropertyChanged(nameof(TotalCarbs));
                RaisePropertyChanged(nameof(TotalFat));
             
            };
            _ = LoadTodayAsync();



            DeleteItemCommand = new AsyncDelegateCommand(
    param => DeleteItemAsync(param as DailyLogItem),
    param => param is DailyLogItem
);

        }

        private async Task EnsureTodayLogIdAsync()
        {
            if (_todayLogId != ObjectId.Empty)
                return;

            var log = await _mongoService.GetOrCreateTodayLogAsync(DateTime.Today);
            _todayLogId = log.Id;
        }

        private async Task LoadTodayAsync()
        {
            var dailyLog = await _mongoService.GetDailyLogByDateAsync(DateTime.Today);
            TodaysItems.Clear();
            if (dailyLog != null && dailyLog.Items != null)
            {
                foreach (var item in dailyLog.Items)
                {
                    TodaysItems.Add(item);
                }
                RaisePropertyChanged(nameof(TotalCalories));
                RaisePropertyChanged(nameof(TotalProtein));
                RaisePropertyChanged(nameof(TotalCarbs));
                RaisePropertyChanged(nameof(TotalFat));
            }
        }

        public async Task AddFoodFromListAsync(Food f, double ConsumedAmount)
        {

            await EnsureTodayLogIdAsync();

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
            await _mongoService.AddItemToDailyLogAsync(_todayLogId, item);
        }

        private async Task DeleteItemAsync(DailyLogItem? item)
        {
            if (item == null)
                return;

            var result = MessageBox.Show($"Are you sure you want to delete {item.Name} from the daily?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            // Find the DailyLogItem in TodaysItems that matches the SelectedItem's FoodId
            var itemToRemove = TodaysItems.FirstOrDefault(i => i.FoodId == item.FoodId);
            if (itemToRemove == null)
                return;

            // Remove from MongoDB
            await _mongoService.RemoveItemFromDailyLogAsync(_todayLogId, itemToRemove);

            // Remove from local collection
            TodaysItems.Remove(itemToRemove);
            SelectedItem = null;
        }



        //public async Task AddFoodToExistingDailyLogAsync(ObjectId dailyLogId, Food food, double amount)
        //{
        //    var factor = amount / food.Amount;
        //    var newItem = new DailyLogItem
        //    {
        //        FoodId = food.Id,
        //        Name = food.Name,
        //        Amount = amount,
        //        Unit = food.Unit,
        //        Time = DateTime.Now,
        //        Calories = (int)Math.Round(factor * food.Calories),
        //        Protein = factor * food.Protein,
        //        Carbs = factor * food.Carbs,
        //        Fat = factor * food.Fat
        //    };

        //    await _mongoService.AddItemToDailyLogAsync(dailyLogId, newItem);
        //}
    }

}








