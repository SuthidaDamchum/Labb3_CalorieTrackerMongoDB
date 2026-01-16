using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Labb3_CalorieTrackerMongoDB.Commands;
using Labb3_CalorieTrackerMongoDB.Models;
using Microsoft.Win32;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;

namespace Labb3_CalorieTrackerMongoDB.ViewModels
{
    class FoodDialogViewModel : ViewModelBase
    {
        private readonly MongoService _mongoService;

        public Food FoodItem { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancleCommand { get; }

        public bool IsEditMode => FoodItem?.Id != ObjectId.Empty;

        public FoodDialogViewModel(MongoService mongoService, Food? food = null)
        {
            _mongoService = mongoService;

            FoodItem = food ?? new Food();


           
        }

        private async Task SaveAsync(object? obj)
        {
            if (IsEditMode)
            {
                var filter = Builders<Food>.Filter.Eq(f => f.Id, FoodItem.Id);
                await _mongoService.Foods.ReplaceOneAsync(filter, FoodItem);
            }

            else
            {
                await _mongoService.Foods.InsertOneAsync(FoodItem);
            }

            if (obj is Window window) window.Close();
        }

        private async Task Cancle(object? obj)
        {
            if (obj is Window window) window.Close();
        }
    }
}
