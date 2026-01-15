using MongoDB.Driver;

namespace Labb3_CalorieTrackerMongoDB.ViewModels
{

    public class MainWindowViewModel
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;

        public MainWindowViewModel()
        {
            var connectionString = "mongodb://localhost:27017/";
            _client = new MongoClient(connectionString);
            _database = _client.GetDatabase("SuthidaDamchum");
        }

    }
}
