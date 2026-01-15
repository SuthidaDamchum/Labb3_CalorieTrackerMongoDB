using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Labb3_CalorieTrackerMongoDB.Models;
using MongoDB.Driver;

namespace Labb3_CalorieTrackerMongoDB.ViewModels
{
    public class FoodViewModel
    {

        static async Task AddFood(Food food)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("SuthidaDamchum");
            var foodCollection = database.GetCollection<Food>("food");

            await foodCollection.InsertOneAsync(food);
        }


    }
}
