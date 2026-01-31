using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Labb3_CalorieTrackerMongoDB.Commands;
using Labb3_CalorieTrackerMongoDB.Models;

namespace Labb3_CalorieTrackerMongoDB.ViewModels
{
        public class MainWindowViewModel : ViewModelBase
        {
        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; RaisePropertyChanged(); }
        }

        public DailyLogViewModel DailyLogVM { get; }
        public FoodViewModel FoodVM { get; }
        public WeeklySummaryViewModel WeeklySummaryVM { get; }
        public ICommand ShowTodaysLogCommand { get; }
        public ICommand ShowFoodListCommand { get; }
        public ICommand ShowWeeklySummaryCommand { get; }
        public MainWindowViewModel()
        {
            var mongoService = new MongoService();

            DailyLogVM = new DailyLogViewModel(mongoService);
            FoodVM = new FoodViewModel(DailyLogVM, mongoService);
            WeeklySummaryVM = new WeeklySummaryViewModel(mongoService);

            CurrentView = DailyLogVM;

            ShowTodaysLogCommand = new AsyncDelegateCommand(_ => { CurrentView = DailyLogVM; return Task.CompletedTask; });
            ShowFoodListCommand = new AsyncDelegateCommand(_ => { CurrentView = FoodVM; return Task.CompletedTask; });
            ShowWeeklySummaryCommand = new AsyncDelegateCommand(_ => { CurrentView = WeeklySummaryVM; return Task.CompletedTask; });
        }
    }
}
