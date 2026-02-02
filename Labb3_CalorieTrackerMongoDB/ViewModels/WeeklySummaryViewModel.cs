using System.Collections.ObjectModel;
using System.Windows.Input;
using Labb3_CalorieTrackerMongoDB.Commands;
using Labb3_CalorieTrackerMongoDB.Models;
using Labb3_CalorieTrackerMongoDB.Services;
using MongoDB.Driver;

namespace Labb3_CalorieTrackerMongoDB.ViewModels
{
    public class WeeklySummaryViewModel : ViewModelBase
    {
        private readonly MongoService _mongoService;

        public ObservableCollection<WeeklySummary> WeekItems { get; } = new();


        private DateTime _weekStartLocal;
        private DateTime _weekEndLocal;

        public string WeekRange => $"{_weekStartLocal:yyyy-MM-dd} → {_weekEndLocal:yyyy-MM-dd}";

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

        public int WeeklyGoalCalories => WeekItems.Sum(x => x.GoalCalories);
        public int WeeklyActualCalories => WeekItems.Sum(x => x.ActualCalories);
        public int WeeklyDiffCalories => WeeklyActualCalories - WeeklyGoalCalories;

        public string WeekCaloriesSummaryText =>
          DaysLogged == 0 ? "No logs yet" :
          $"Total {WeeklyActualCalories} / Goal {WeeklyGoalCalories} (Diff {WeeklyDiffCalories:+#;-#;0})";

        public ICommand PrevWeekCommand { get; }
        public ICommand NextWeekCommand { get; }
        public ICommand ThisWeekCommand { get; }


        public WeeklySummaryViewModel(MongoService mongoService)
        {
            _mongoService = mongoService;


            _weekStartLocal = GetWeekStartLocalMonday(DateTime.Today);
            _weekEndLocal = _weekStartLocal.AddDays(6);


            AppEvents.DailyLogChanged += OnDailyLogChangedAsync;

            PrevWeekCommand = new AsyncDelegateCommand(async _ => await MoveWeekAsync(-7));
            NextWeekCommand = new AsyncDelegateCommand(async _ => await MoveWeekAsync(+7));
            ThisWeekCommand = new AsyncDelegateCommand(async _ => await GoToThisWeekAsync());

            _ = LoadAsync();
        }

        private static DateTime GetWeekStartLocalMonday(DateTime localDate)
        {
            localDate = localDate.Date;
            int diff = (7 + ((int)localDate.DayOfWeek - (int)DayOfWeek.Monday)) % 7;
            return localDate.AddDays(-diff);
        }

        private async Task MoveWeekAsync(int days)
        {
            _weekStartLocal = _weekStartLocal.AddDays(days);
            _weekEndLocal = _weekStartLocal.AddDays(6);
            await LoadAsync();
        }

        private async Task GoToThisWeekAsync()
        {
            _weekStartLocal = GetWeekStartLocalMonday(DateTime.Today);
            _weekEndLocal = _weekStartLocal.AddDays(6);
            await LoadAsync();
        }

        private async Task OnDailyLogChangedAsync()
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {

            var weekStartUtc = _mongoService.GetWeekStartUtcStockholm(_weekStartLocal);
            var weekEndUtc = weekStartUtc.AddDays(7);

            var wg = await _mongoService.GetWeeklyGoalForDateAsync(_weekStartLocal);

            int goalKcal = wg?.GoalCalories ?? 0;
            int goalP = (int)Math.Round(wg?.GoalProtein ?? 0);
            int goalC = (int)Math.Round(wg?.GoalCarbs ?? 0);
            int goalF = (int)Math.Round(wg?.GoalFat ?? 0);

            var logs = await _mongoService.DailyLogs
                .Find(x => x.Date >= weekStartUtc && x.Date < weekEndUtc)
                .SortBy(x => x.Date)
                .ToListAsync();

            var tz = _mongoService.StockholmTz;
            var byLocalDate = logs.ToDictionary(
                l => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(l.Date, DateTimeKind.Utc), tz).Date
            );

            WeekItems.Clear();

            for (int i = 0; i < 7; i++)
            {
                var date = _weekStartLocal.AddDays(i).Date;

                if (byLocalDate.TryGetValue(date, out var log))
                {
                    var items = log.Items ?? new();

                    WeekItems.Add(new WeeklySummary
                    {
                        Date = date,
                        HasLog = items.Count > 0,

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

                        GoalCalories = goalKcal,
                        GoalProtein = goalP,
                        GoalCarbs = goalC,
                        GoalFat = goalF,

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
            RaisePropertyChanged(nameof(WeekCaloriesSummaryText));
        }
    }

}

