using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Labb3_CalorieTrackerMongoDB.Commands;
using Labb3_CalorieTrackerMongoDB.Dialogs;
using Labb3_CalorieTrackerMongoDB.Models;
using Labb3_CalorieTrackerMongoDB.Services;
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
        public ICommand OpenSetNewGoalDiaLogCommand { get; }
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


                RaisePropertyChanged(nameof(GoalCalories));
                RaisePropertyChanged(nameof(GoalProtein));
                RaisePropertyChanged(nameof(GoalCarbs));
                RaisePropertyChanged(nameof(GoalFat));
            }
        }

        public int GoalCalories => DailyLog?.GoalCalories ?? 0;
        public int GoalProtein => DailyLog?.GoalProtein ?? 0;
        public int GoalCarbs => DailyLog?.GoalCarbs ?? 0;
        public int GoalFat => DailyLog?.GoalFat ?? 0;


        public int TotalCalories => TodaysItems.Sum(f => f.Calories);
        public int TotalProtein => (int)Math.Round(TodaysItems.Sum(f => f.Protein));
        public int TotalCarbs => (int)Math.Round(TodaysItems.Sum(f => f.Carbs));
        public int TotalFat => (int)Math.Round(TodaysItems.Sum(f => f.Fat));

        public DailyLogViewModel(MongoService mongoService)
        {
            _mongoService = mongoService;

            TodaysItems.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (DailyLogItem item in e.NewItems)
                        item.PropertyChanged += DailyLogItem_PropertyChanged;
                }
                if (e.OldItems != null)
                {
                    foreach (DailyLogItem item in e.OldItems)
                        item.PropertyChanged -= DailyLogItem_PropertyChanged;
                }
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

            OpenSetNewGoalDiaLogCommand = new AsyncDelegateCommand(_ => OpenSetNewGoalDialogAsync());


        }
        private async Task OpenSetNewGoalDialogAsync()
        {
            await EnsureLogExistsForDateAsync(SelectedDate);

            var dialog = new GoalDialog();

            dialog.DataContext = new SetNewGoalViewModel(
                _mongoService,
                SelectedDate,
                new SetNewGoal
                {
                    GoalCalories = DailyLog.GoalCalories,
                    GoalProtein = DailyLog.GoalProtein,
                    GoalCarbs = DailyLog.GoalCarbs,
                    GoalFat = DailyLog.GoalFat
                });

            var result = dialog.ShowDialog();

            if (dialog.DataContext is SetNewGoalViewModel vm && vm.DialogResult == true)
            {

                var refreshed = await _mongoService.GetDailyLogByDateAsync(SelectedDate);

                DailyLog = refreshed ?? new DailyLog
                {
                    Date = SelectedDate.Date,
                    Items = new List<DailyLogItem>()
                };

                TodaysItems.Clear();
                if (DailyLog.Items != null)

                    foreach (var item in DailyLog.Items)
                        TodaysItems.Add(item);

                RaisePropertyChanged(nameof(GoalCalories));
                RaisePropertyChanged(nameof(GoalProtein));
                RaisePropertyChanged(nameof(GoalCarbs));
                RaisePropertyChanged(nameof(GoalFat));


            }
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

                DailyLog = new DailyLog { Date = todayDate, Items = new List<DailyLogItem>() };
            }
            System.Diagnostics.Debug.WriteLine(

            dailyLog == null ? "dailyLog = null" : $"dailyLog items: {dailyLog.Items?.Count}"
        );
            RaisePropertyChanged(nameof(TotalCalories));
            RaisePropertyChanged(nameof(TotalProtein));
            RaisePropertyChanged(nameof(TotalCarbs));
            RaisePropertyChanged(nameof(TotalFat));
        }

        private async Task LoadLogForDateAsync()
        {
            var date = SelectedDate.Date;

            var dailyLog = await _mongoService.GetOrCreateTodayLogAsync(date);

            DailyLog = dailyLog;

            TodaysItems.Clear();

            if (dailyLog.Items != null)
            {

                foreach (var item in dailyLog.Items)

                    TodaysItems.Add(item);

                RaisePropertyChanged(nameof(TotalCalories));
                RaisePropertyChanged(nameof(TotalProtein));
                RaisePropertyChanged(nameof(TotalCarbs));
                RaisePropertyChanged(nameof(TotalFat));

                await AppEvents.RaiseDailyLogChangedAsync();
            }
        }
        public async Task AddFoodFromListAsync(Food f, double ConsumedAmount)
        {

            await EnsureLogExistsForDateAsync(SelectedDate);
            var factor = ConsumedAmount / f.Amount;
            var existingItem = TodaysItems.FirstOrDefault(i => i.FoodId == f.Id);

            if (existingItem != null)
            {

                existingItem.Amount += ConsumedAmount;
                existingItem.Time = DateTime.Now;
                existingItem.RecalculateFromAmount();

                await _mongoService.UpdateItemInDailyLogAsync(DailyLog.Id, existingItem);
            }
            else
            {
                var item = new DailyLogItem
                {
                    FoodId = f.Id,
                    Name = f.Name,

                    Unit = f.Unit,
                    BaseUnit = f.Unit,
                    LogUnit = f.Unit,


                    BaseAmount = f.Amount,
                    BaseCalories = f.Calories,
                    BaseProtein = f.Protein,
                    BaseCarbs = f.Carbs,
                    BaseFat = f.Fat,
                    Amount = ConsumedAmount,
                    Time = DateTime.Now
                };

                item.RecalculateFromAmount();

                TodaysItems.Add(item);


                await _mongoService.AddItemToDailyLogAsync(DailyLog.Id, item);

            }

            RaisePropertyChanged(nameof(TotalCalories));
            RaisePropertyChanged(nameof(TotalProtein));
            RaisePropertyChanged(nameof(TotalCarbs));
            RaisePropertyChanged(nameof(TotalFat));


            await AppEvents
                 .RaiseDailyLogChangedAsync();

        }

        private async Task EnsureLogExistsForDateAsync(DateTime date)
        {
            var localDate = date.Date;

            if (DailyLog != null && DailyLog.Id != ObjectId.Empty)
            {
                var existing = await _mongoService.GetDailyLogByDateAsync(localDate);
                if (existing != null && existing.Id == DailyLog.Id)
                    return;
            }
            DailyLog = await _mongoService.GetOrCreateTodayLogAsync(localDate);
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

            await _mongoService.RemoveItemFromDailyLogAsync(DailyLog.Id, item.FoodId);
            var local = TodaysItems.FirstOrDefault(x => x.FoodId == item.FoodId);
            if (local != null)
                TodaysItems.Remove(local);

            SelectedItem = null;


            RaisePropertyChanged(nameof(TotalCalories));
            RaisePropertyChanged(nameof(TotalProtein));
            RaisePropertyChanged(nameof(TotalCarbs));
            RaisePropertyChanged(nameof(TotalFat));

            await AppEvents
            .RaiseDailyLogChangedAsync();

        }
        private async Task UpdateItemAmountAsync(DailyLogItem item)
        {
            await _mongoService.UpdateItemInDailyLogAsync(DailyLog.Id, item);
            RaisePropertyChanged(nameof(TotalCalories));
            RaisePropertyChanged(nameof(TotalProtein));
            RaisePropertyChanged(nameof(TotalCarbs));
            RaisePropertyChanged(nameof(TotalFat));


            await AppEvents
            .RaiseDailyLogChangedAsync();
        }           

        private async void DailyLogItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DailyLogItem.Amount) && sender is DailyLogItem item)
            {
                await UpdateItemAmountAsync(item);
            }
        }
    }

}


