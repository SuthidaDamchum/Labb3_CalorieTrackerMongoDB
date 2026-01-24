using System.Collections.ObjectModel;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
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
        public DateTime TodayDate => DailyLog?.Date ?? DateTime.Today;

        public ICommand DeleteItemCommand { get; }

        public ICommand LoadLogForDateCommand { get; }

        private DateTime _selectedDated = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDated;
            set
            {
                _selectedDated = value;
                RaisePropertyChanged();
                _ = LoadLogForDateAsync();

            }
        }

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
        private DailyLog _dailyLog;
        public DailyLog DailyLog
        {
            get => _dailyLog;

            set
            {
                _dailyLog = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(TodayDate));
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

            LoadLogForDateCommand = new AsyncDelegateCommand(_ => LoadLogForDateAsync());
        }



        private async Task EnsureTodayLogExistsAsync()
        {
            if (DailyLog != null && DailyLog.Id != ObjectId.Empty)
                return;

            var todayDate = DateTime.Today.Date;

 
            DailyLog = await _mongoService.GetOrCreateTodayLogAsync(todayDate);

        }

        private async Task LoadTodayAsync()
        {
            var todayDate = DateTime.Today.Date;
            var dailyLog = await _mongoService.GetDailyLogByDateAsync(todayDate);

            TodaysItems.Clear();
            if (dailyLog != null && dailyLog.Items != null)
            {
                DailyLog = dailyLog;
                foreach (var item in dailyLog.Items)
                {
                    TodaysItems.Add(item);
                }
            }
            else
            {
                // ✅ så binding alltid har något
                DailyLog = new DailyLog { Date = todayDate, Items = new List<DailyLogItem>() };
            }

            RaisePropertyChanged(nameof(TotalCalories));
            RaisePropertyChanged(nameof(TotalProtein));
            RaisePropertyChanged(nameof(TotalCarbs));
            RaisePropertyChanged(nameof(TotalFat));
        }


        private async Task LoadLogForDateAsync()
        {
            var selectedDate = SelectedDate.Date;

            var log = await _mongoService.GetDailyLogByDateAsync(selectedDate);

            TodaysItems.Clear();
            if (log != null && log.Items != null)
            {
                DailyLog = log;
                foreach (var item in log.Items)
                    TodaysItems.Add(item);
            }
            else
            {
                DailyLog = new DailyLog { Date = selectedDate, Items = new List<DailyLogItem>() };
            }
            RaisePropertyChanged(nameof(TotalCalories));
            RaisePropertyChanged(nameof(TotalProtein));
            RaisePropertyChanged(nameof(TotalCarbs));
            RaisePropertyChanged(nameof(TotalFat));
        }


        public async Task AddFoodFromListAsync(Food f, double ConsumedAmount)
        {
            await EnsureTodayLogExistsAsync();

            var factor = ConsumedAmount / f.Amount;
            var existingItem = TodaysItems.FirstOrDefault(i => i.FoodId == f.Id);

            if (existingItem != null)
            {
                existingItem.Amount += ConsumedAmount;
                existingItem.Calories += (int)Math.Round(factor * f.Calories);
                existingItem.Protein += factor * f.Protein;
                existingItem.Carbs += factor * f.Carbs;
                existingItem.Fat += factor * f.Fat;
                existingItem.Time = DateTime.Now;

                await _mongoService.UpdateItemInDailyLogAsync(DailyLog.Id, existingItem);
                RaisePropertyChanged(nameof(TotalCalories));
                RaisePropertyChanged(nameof(TotalProtein));
                RaisePropertyChanged(nameof(TotalCarbs));
                RaisePropertyChanged(nameof(TotalFat));
            }
            else
            {
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
                await _mongoService.AddItemToDailyLogAsync(DailyLog.Id, item);
            }
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
            await _mongoService.RemoveItemFromDailyLogAsync(DailyLog.Id, itemToRemove);

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

