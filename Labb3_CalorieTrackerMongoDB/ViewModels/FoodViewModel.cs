using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Labb3_CalorieTrackerMongoDB.Commands;
using Labb3_CalorieTrackerMongoDB.Dialogs;
using Labb3_CalorieTrackerMongoDB.Models;
using MongoDB.Driver;

namespace Labb3_CalorieTrackerMongoDB.ViewModels
{
    public class FoodViewModel : ViewModelBase
    {
        private readonly MongoService _mongoService;
        private readonly DailyLogViewModel _todaysLogVM;

        public ObservableCollection<Food> Foods { get; set; }
        public ICommand LoadFoodsCommand { get; }
        public ICommand AddFoodCommand { get; }
        public ICommand AddToTodayCommand { get; }
        public ICommand EditFoodCommand { get; }

        public FoodViewModel(DailyLogViewModel todaysLogVM, MongoService mongoService)
        {
            _todaysLogVM = todaysLogVM;
            _mongoService = mongoService;

            Foods = new ObservableCollection<Food>();

            LoadFoodsCommand = new AsyncDelegateCommand(_ => LoadFoodsAsync());
            AddFoodCommand = new AsyncDelegateCommand(_ => OpenFoodDialogAsync());
            EditFoodCommand = new AsyncDelegateCommand(food => OpenFoodDialogAsync(food as Food));
            AddToTodayCommand = new DelegateCommand(food =>
            {
                if (food is Food f)
                    _todaysLogVM.TodaysFood.Add(f);
            });

            //load foods at startup
            LoadFoodsCommand.Execute(null);
        }

        private async Task LoadFoodsAsync()
        {
            var foods = await _mongoService.Foods.Find(_ => true).ToListAsync();
            Foods.Clear();
            foreach (var food in foods)
                Foods.Add(food);
        }


        private async Task OpenFoodDialogAsync(Food? food = null)
        {
            var dialog = new Add_Edit_FoodDailog();
            var vm = new FoodDialogViewModel(_mongoService, food);
            dialog.DataContext = vm;

            //show dialog 
            dialog.ShowDialog();

            //Reload foods after closing dialog
            await LoadFoodsAsync();
        }
    }
}
