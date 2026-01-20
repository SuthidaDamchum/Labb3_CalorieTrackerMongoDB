using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Labb3_CalorieTrackerMongoDB.Commands;
using Labb3_CalorieTrackerMongoDB.Dialogs;
using Labb3_CalorieTrackerMongoDB.Models;
using MongoDB.Bson;
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
        public ICommand DeleteCommand { get; }
        public ICommand UpdateFoodCommand { get; }

        private Food? _selectedFood;
        public Food? SelectedFood
        {
            get => _selectedFood;
            set
            {
                _selectedFood = value;
                RaisePropertyChanged();
                (DeleteCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
            }
        }

        public FoodViewModel(DailyLogViewModel todaysLogVM, MongoService mongoService)
        {
            _todaysLogVM = todaysLogVM;
            _mongoService = mongoService;

            Foods = new ObservableCollection<Food>();

            DeleteCommand = new AsyncDelegateCommand(
                 DeleteAsync,
                 _ => SelectedFood != null
             );


            LoadFoodsCommand = new AsyncDelegateCommand(_ => LoadFoodsAsync());

            AddFoodCommand = new AsyncDelegateCommand(_ => OpenFoodDialogAsync());

            UpdateFoodCommand = new AsyncDelegateCommand(
                    food => OpenFoodDialogAsync(food as Food),
                 _ => SelectedFood != null
                );

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


            dialog.ShowDialog();


            await LoadFoodsAsync();
        }


        private async Task DeleteAsync(object? obj)
        {
            if (SelectedFood == null)
                return;

            var result = MessageBox.Show($"Are you sure you want to delete {SelectedFood.Name} from the list?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            var filter = Builders<Food>.Filter.Eq(f => f.Id, SelectedFood.Id);
            await _mongoService.Foods.DeleteOneAsync(filter);

            Foods.Remove(SelectedFood);

        }

   
        }
    }
