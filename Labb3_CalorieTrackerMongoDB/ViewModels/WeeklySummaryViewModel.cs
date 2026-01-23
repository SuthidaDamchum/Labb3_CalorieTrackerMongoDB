using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Labb3_CalorieTrackerMongoDB.Commands;
using Labb3_CalorieTrackerMongoDB.Models;
using MongoDB.Driver;

namespace Labb3_CalorieTrackerMongoDB.ViewModels
{
    public class WeeklySummaryViewModel : ViewModelBase
    {
        private readonly MongoService _mongoService;
        public ObservableCollection<DailyLogSummary> WeeklyLogs { get; set; } = new();
        public ICommand LoadWeeklyLogsCommand { get; }
        public WeeklySummaryViewModel(MongoService mongoService)
        {
            _mongoService = mongoService;
            LoadWeeklyLogsCommand = new AsyncDelegateCommand(_ => LoadWeeklyLogsAsync());

            _ = LoadWeeklyLogsAsync();
        }

        private async Task LoadWeeklyLogsAsync()
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1); // Monday
            var endOfWeek = startOfWeek.AddDays(6);

            var logs = await _mongoService.DailyLogs
                .Find(log => log.Date >= startOfWeek && log.Date <= endOfWeek)
                .SortBy(log => log.Date)
                .ToListAsync();

            WeeklyLogs.Clear();

            foreach (var log in logs)
            {
                WeeklyLogs.Add(new DailyLogSummary
                {
                    Date = log.Date,
                    ActualCalories = log.TotalCalories,
                    Protein = log.TotalProtein,
                    Carbs = log.TotalCarbs,
                    Fat = log.TotalFat
                });
            }
        }
        public string WeekRange
            => $"Week: {CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Today, CalendarWeekRule.FirstDay, DayOfWeek.Monday)} " +
            $"({WeeklyLogs.FirstOrDefault()?.Date:yyyy-MM-dd} → {WeeklyLogs.LastOrDefault()?.Date:yyyy-MM-dd})";
    }
}
