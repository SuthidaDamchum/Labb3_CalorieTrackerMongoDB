using System.Collections.ObjectModel;
using Labb3_CalorieTrackerMongoDB.Models;
using MongoDB.Driver;

namespace Labb3_CalorieTrackerMongoDB.ViewModels
{
    public class WeeklySummaryViewModel : ViewModelBase
    {
        private readonly MongoService _mongoService;

        public ObservableCollection<WeeklySummary> WeekItems { get; } = new();


        private DateTime _start;
        private DateTime _end;

        private const int DbDayOffset = 1;

        public string WeekRange => $"{_start:yyyy-MM-dd} → {_end:yyyy-MM-dd}";

        public int DaysLogged => WeekItems.Count(x => x.HasLog);
        public int DaysOnTarget => WeekItems.Count(x => x.HasLog && x.ActualCalories <= x.GoalCalories);

        public double AvgCalories
        {
            get
            {
                var logged = WeekItems.Where(x => x.HasLog).ToList();
                return logged.Count == 0 ? 0 : logged.Average(x => x.ActualCalories);
            }
        }

        public WeeklySummaryViewModel(MongoService mongoService)
        {
            _mongoService = mongoService;

            AppEvents.DailyLogChanged += OnDailyLogChangedAsync;

            _ = LoadAsync();
        }

  
        private async Task OnDailyLogChangedAsync()
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            var today = DateTime.Today;
            int diff = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;

            _start = today.AddDays(-diff).Date;
            _end = _start.AddDays(6).Date;


            var weekStartUtc = _mongoService.GetWeekStartUtcStockholm(_start); 
            var weekEndUtc = weekStartUtc.AddDays(7);

            var logs = await _mongoService.DailyLogs
                .Find(x => x.Date >= weekStartUtc && x.Date < weekEndUtc)
                .SortBy(x => x.Date)
                .ToListAsync();

     
            var byDate = logs.ToDictionary(
                l => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(l.Date, DateTimeKind.Utc), _mongoService.StockholmTz).Date
            );

            WeekItems.Clear();

            for (int i = 0; i < 7; i++)
            {
                var date = _start.AddDays(i).Date;

                if (byDate.TryGetValue(date, out var log))
                {
                    var items = log.Items ?? new();

                    WeekItems.Add(new WeeklySummary
                    {
                        Date = date,
                        HasLog = true,

                        GoalCalories = log.GoalCalories,
                        GoalProtein = log.GoalProtein,
                        GoalCarbs = log.GoalCarbs,
                        GoalFat = log.GoalFat,

                        ActualCalories = items.Sum(x => x.Calories),
                        ActualProtein = (int)Math.Round(items.Sum(x => x.Protein)),
                        ActualCarbs = (int)Math.Round(items.Sum(x => x.Carbs)),
                        ActualFat = (int)Math.Round(items.Sum(x => x.Fat)),
                    });
                }
                else
                {
                    WeekItems.Add(new WeeklySummary
                    {
                        Date = date,
                        HasLog = false,
                        GoalCalories = 0,
                        GoalProtein = 0,
                        GoalCarbs = 0,
                        GoalFat = 0,
                        ActualCalories = 0,
                        ActualProtein = 0,
                        ActualCarbs = 0,
                        ActualFat = 0
                    });
                }
            }

            RaisePropertyChanged(nameof(WeekRange));
            RaisePropertyChanged(nameof(DaysLogged));
            RaisePropertyChanged(nameof(DaysOnTarget));
            RaisePropertyChanged(nameof(AvgCalories));
        }
}


        public static class AppEvents
    {
        public static event Func<Task>? DailyLogChanged;

        public static void RaiseDailyLogChanged()
        {
            DailyLogChanged?.Invoke();
        }
    }
}
