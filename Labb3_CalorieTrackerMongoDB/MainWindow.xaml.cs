using System.Windows;
using Labb3_CalorieTrackerMongoDB.ViewModels;
using MongoDB.Driver;

namespace Labb3_CalorieTrackerMongoDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new MainWindowViewModel();
        }
    }

}

