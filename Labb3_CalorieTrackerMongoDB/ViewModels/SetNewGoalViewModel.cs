using System.Globalization;
using System.Windows.Input;
using Labb3_CalorieTrackerMongoDB.Commands;
using Labb3_CalorieTrackerMongoDB.Models;
using Labb3_CalorieTrackerMongoDB.Services;

namespace Labb3_CalorieTrackerMongoDB.ViewModels
{
    public class SetNewGoalViewModel : ViewModelBase

    {

        private readonly DateTime _date;
        private readonly MongoService _mongoService;

        private bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            set { _dialogResult = value; RaisePropertyChanged(); }
        }

        private string _goalCaloriesText = "";
        public string GoalCaloriesText
        {
            get => _goalCaloriesText;
            set
            {
                if (_goalCaloriesText == value) return;
                _goalCaloriesText = value;
                RaisePropertyChanged();
                Validate();
            }
        }

        private string _goalProteinText = "";
        public string GoalProteinText
        {
            get => _goalProteinText;
            set
            {
                if (_goalProteinText == value) return;
                _goalProteinText = value;
                RaisePropertyChanged();
                Validate();
            }
        }

        private string _goalCarbsText = "";
        public string GoalCarbsText
        {
            get => _goalCarbsText;
            set
            {
                if (_goalCarbsText == value) return;
                _goalCarbsText = value;
                RaisePropertyChanged();
                Validate();
            }
        }

        private string _goalFatText = "";
        public string GoalFatText
        {
            get => _goalFatText;
            set
            {
                if (_goalFatText == value) return;
                _goalFatText = value;
                RaisePropertyChanged();
                Validate();
            }
        }

        private string _errorMessage = " ";
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; RaisePropertyChanged(); }
        }

        private bool _canSave;
        public bool CanSave
        {
            get => _canSave;
            set { _canSave = value; RaisePropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public SetNewGoalViewModel(MongoService mongoService, DateTime date, SetNewGoal? existing = null)
        {
            _mongoService = mongoService ?? throw new ArgumentNullException(nameof(mongoService));
            _date = date.Date;

            if (existing != null)
            {
                GoalCaloriesText = existing.GoalCalories.ToString(CultureInfo.InvariantCulture);
                GoalProteinText = existing.GoalProtein.ToString(CultureInfo.InvariantCulture);
                GoalCarbsText = existing.GoalCarbs.ToString(CultureInfo.InvariantCulture);
                GoalFatText = existing.GoalFat.ToString(CultureInfo.InvariantCulture);
            }

            SaveCommand = new AsyncDelegateCommand(_ => SaveAsync());
            CancelCommand = new DelegateCommand(_ => DialogResult = false);

            Validate();
        }

        private async Task SaveAsync()
        {
            if (!TryBuildModel(out var model))
            {
                Validate();
                return;
            }

            try
            {

                await _mongoService.UpsertWeeklyGoalAsync(_date, model);




                await _mongoService.UpdateDailyGoalsForWeekAsync(_date, model);

                await AppEvents.RaiseDailyLogChangedAsync();
                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to save weekly goals: " + ex.Message;
                CanSave = false;
            }
        }

        public bool TryBuildModel(out SetNewGoal model)
        {
            model = new SetNewGoal();

            if (!int.TryParse(GoalCaloriesText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var kcal) || kcal <= 0)
                return false;

            if (!double.TryParse(GoalProteinText, NumberStyles.Float, CultureInfo.InvariantCulture, out var p) || p < 0)
                return false;

            if (!double.TryParse(GoalCarbsText, NumberStyles.Float, CultureInfo.InvariantCulture, out var c) || c < 0)
                return false;

            if (!double.TryParse(GoalFatText, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) || f < 0)
                return false;

            model.GoalCalories = kcal;
            model.GoalProtein = p;
            model.GoalCarbs = c;
            model.GoalFat = f;

            return true;
        }

        private void Validate()
        {
            if (TryBuildModel(out _))
            {
                ErrorMessage = " ";
                CanSave = true;
            }
            else
            {
                ErrorMessage = "Enter valid numbers (Calories > 0, macros ≥ 0).";
                CanSave = false;
            }
        }
    }
}