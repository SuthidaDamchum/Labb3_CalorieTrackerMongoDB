using System.Collections.ObjectModel;
using System.Windows;
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
        public ObservableCollection<Food> Foods { get; set; } = new ObservableCollection<Food>();
        public ICommand LoadFoodsCommand { get; }
        public ICommand AddFoodCommand { get; }
        public ICommand UpdateFoodCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddToTodayCommand { get; }

        private Food? _selectedFood;
        public Food? SelectedFood
        {
            get => _selectedFood;
            set
            {
                _selectedFood = value;
                RaisePropertyChanged();
                (DeleteCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
                (AddToTodayCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                (UpdateFoodCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
            }
        }

        public FoodViewModel(DailyLogViewModel todaysLogVM, MongoService mongoService)
        {
            _todaysLogVM = todaysLogVM;

            _mongoService = mongoService;
     

            DeleteCommand = new AsyncDelegateCommand(
               _ =>  DeleteAsync(),
                 _ => SelectedFood != null
             );

            LoadFoodsCommand = new AsyncDelegateCommand(_ => LoadFoodsAsync());

            AddFoodCommand = new AsyncDelegateCommand(_ => OpenFoodDialogAsync());

            UpdateFoodCommand = new AsyncDelegateCommand(
                    _ => OpenFoodDialogAsync(SelectedFood),
                 _ => SelectedFood != null
                );

            AddToTodayCommand = new DelegateCommand(
                food => AddSelectedFoodToToday(),
                _ => SelectedFood != null
);
               
            // Correct way to load foods at startup (fire and forget)
            Task.Run(() => LoadFoodsAsync());
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

        private async Task DeleteAsync()
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
            SelectedFood = null;
        }

        private void AddSelectedFoodToToday()
        {
            if (SelectedFood == null)
                return;

            _todaysLogVM.AddFoodFromList(
                SelectedFood,
                SelectedFood.Amount
        );

         
        }
    }
}
