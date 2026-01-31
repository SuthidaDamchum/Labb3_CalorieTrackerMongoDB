using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Labb3_CalorieTrackerMongoDB.Models
{
    public class DailyLogItem : INotifyPropertyChanged
    {
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public ObjectId FoodId { get; set; }
        public string Name { get; set; } = "";
        [BsonRepresentation(BsonType.String)]
        public Unit Unit { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public int Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fat { get; set; }

        public Unit BaseUnit { get; set; }  

        public double BaseAmount { get; set; } = 1;    
        public int BaseCalories { get; set; }
        public double BaseProtein { get; set; }
        public double BaseCarbs { get; set; }
        public double BaseFat { get; set; }

        private double _amount;
        public double Amount
        {
            get => _amount;
            set
            {
                if (Math.Abs(_amount - value) < 0.0001)
                    return;

                _amount = value;
                Time = DateTime.Now;

                RecalculateFromAmount(); 

                OnPropertyChanged(nameof(Amount));
                OnPropertyChanged(nameof(Time));
                OnPropertyChanged(nameof(Calories));
                OnPropertyChanged(nameof(Protein));
                OnPropertyChanged(nameof(Carbs));
                OnPropertyChanged(nameof(Fat));
            }
        }

        public Unit LogUnit { get; set; }    
        private Unit _logUnit
    
        {
            get => _logUnit;
            set
            {
                if (_logUnit == value) return;
                _logUnit = value;

  

                OnPropertyChanged(nameof(LogUnit));
                RecalculateFromAmount();
                OnPropertyChanged(nameof(Calories));
                OnPropertyChanged(nameof(Protein));
                OnPropertyChanged(nameof(Carbs));
                OnPropertyChanged(nameof(Fat));
            }
        }


        public void RecalculateFromAmount()
        {
           
            if (BaseAmount <= 0 || BaseCalories <= 0)
            {
                BaseAmount = Amount <= 0 ? 1 : Amount;
                BaseCalories = Calories;
                BaseProtein = Protein;
                BaseCarbs = Carbs;
                BaseFat = Fat;
            }

            var baseAmt = BaseAmount <= 0 ? 1 : BaseAmount;
            var factor = Amount / baseAmt;

            Calories = (int)Math.Round(BaseCalories * factor);
            Protein = Math.Round(BaseProtein * factor, 2);
            Carbs = Math.Round(BaseCarbs * factor, 2);
            Fat = Math.Round(BaseFat * factor, 2);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}