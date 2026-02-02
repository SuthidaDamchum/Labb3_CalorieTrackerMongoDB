using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Labb3_CalorieTrackerMongoDB.Commands;
using Labb3_CalorieTrackerMongoDB.Dialogs;
using Labb3_CalorieTrackerMongoDB.Models;
using Labb3_CalorieTrackerMongoDB.Services;
using MongoDB.Driver;

namespace Labb3_CalorieTrackerMongoDB.ViewModels
{
    public class FoodViewModel : ViewModelBase

    {
        private readonly MongoService _mongoService;
        private readonly DailyLogViewModel _CurrentLogVM; public ObservableCollection<Food> Foods { get; set; } = new ObservableCollection<Food>();


        public ICommand LoadFoodsCommand { get; }
        public ICommand AddFoodCommand { get; }
        public ICommand UpdateFoodCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddToTodayCommand { get; }


        private double _consumedAmount;
        public double ConsumedAmount
        {
            get => _consumedAmount;
            set
            {
                _consumedAmount = value;
                RaisePropertyChanged();
            }
        }

        private Food? _selectedFood;
        public Food? SelectedFood
        {
            get => _selectedFood;
            set
            {
                _selectedFood = value;
                RaisePropertyChanged();

                if (_selectedFood != null)
                {
                    ConsumedAmount = _selectedFood.Amount;
                }

                (DeleteCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
                (AddToTodayCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
                (UpdateFoodCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();

            }
        }
        public FoodViewModel(DailyLogViewModel todaysLogVM, MongoService mongoService)
        {
            _CurrentLogVM = todaysLogVM;

            _mongoService = mongoService;


            DeleteCommand = new AsyncDelegateCommand(
               _ => DeleteAsync(),
                 _ => SelectedFood != null
             );

            LoadFoodsCommand = new AsyncDelegateCommand(_ => LoadFoodsAsync());

            AddFoodCommand = new AsyncDelegateCommand(_ => OpenFoodDialogAsync());

            UpdateFoodCommand = new AsyncDelegateCommand(
                    _ => OpenFoodDialogAsync(SelectedFood),
                 _ => SelectedFood != null
                );

            AddToTodayCommand = new AsyncDelegateCommand(
                food => AddSelectedFoodToTodayAsync(),
                _ => SelectedFood != null
        );


            Task.Run(() => LoadFoodsAsync());
        }
        private async Task LoadFoodsAsync()
        {
            var foods = await _mongoService.Foods.Find(_ => true).ToListAsync();
            Foods.Clear();
            foreach (var food in foods)
                Foods.Add(food);
            RaisePropertyChanged();
        }
        private async Task OpenFoodDialogAsync(Food? food = null)
        {
            var dialog = new Add_Edit_FoodDialog();
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
            SelectedFood = null;
        }
        private async Task AddSelectedFoodToTodayAsync()
        {
            if (SelectedFood == null)
                return;

            var amount = ConsumedAmount <= 0 ? SelectedFood.Amount : ConsumedAmount;


            await _CurrentLogVM.AddFoodFromListAsync(SelectedFood, amount);

        }
    }
}
