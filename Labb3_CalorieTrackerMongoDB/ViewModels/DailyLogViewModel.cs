using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Labb3_CalorieTrackerMongoDB.Commands;
using Labb3_CalorieTrackerMongoDB.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Labb3_CalorieTrackerMongoDB.ViewModels
{
    public class DailyLogViewModel : ViewModelBase
    {
        public ObservableCollection<Food> TodaysFood { get; } = new ObservableCollection<Food>();

        private readonly MongoService _mongoService;

 
        
        public ICommand SaveDayCommand { get; }

        public DateTime TodayDate => DateTime.Today;
        public int GoalCalories => 1500;
        public int GoalProtein => 100;
        public int GoalCarbs => 150;
        public int GoalFat => 50;

        // Totals som uppdateras när du lägger till mat
        public int TotalCalories => TodaysFood.Sum(f => f.Calories);
        public int TotalProtein => TodaysFood.Sum(f => f.Protein);
        public int TotalCarbs => TodaysFood.Sum(f => f.Carbs);
        public int TotalFat => TodaysFood.Sum(f => f.Fat);


        public DailyLogViewModel(MongoService mongoService)
        {
            _mongoService = mongoService;
            TodaysFood = new ObservableCollection<Food>();

            SaveDayCommand = new AsyncDelegateCommand(_ => SaveDayAsync(), _=> TodaysFood.Any());

            // Om du vill uppdatera totals när listan ändras
            TodaysFood.CollectionChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(TotalCalories));
                RaisePropertyChanged(nameof(TotalProtein));
                RaisePropertyChanged(nameof(TotalCarbs));
                RaisePropertyChanged(nameof(TotalFat));
            };
        }

        

        private async Task SaveDayAsync()
        {
            var dailyLog = new DailyLog
            {

                Date = TodayDate,
                Foods = TodaysFood.ToList(),
                TotalCalories = TotalCalories,
                TotalProtein = TotalProtein,
                TotalCarbs = TotalCarbs,
                TotalFat = TotalFat
            };

            await _mongoService.DailyLogs.InsertOneAsync(dailyLog);
        }
    }

}


