using System.Configuration;
using System.Data;
using System.Windows;
using Labb3_CalorieTrackerMongoDB.Models;
using Labb3_CalorieTrackerMongoDB.Services;
using Labb3_CalorieTrackerMongoDB.ViewModels;
using MongoDB.Driver;

namespace Labb3_CalorieTrackerMongoDB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mongoService = new MongoService();
            var databaseSeeder = new DatabaseSeeder(mongoService);
            await databaseSeeder.SeedAsync();
            
            var dailyVM = new DailyLogViewModel(mongoService);
            var foodVM = new FoodViewModel(dailyVM, mongoService);
            var weeklySummaryVM = new WeeklySummaryViewModel(mongoService);

            var mainViewModel = new MainWindowViewModel(dailyVM, foodVM, weeklySummaryVM);

            var mainView = new MainWindow()
            {
                DataContext = mainViewModel
            };

            mainView.Show();
        }
      

    }
}
