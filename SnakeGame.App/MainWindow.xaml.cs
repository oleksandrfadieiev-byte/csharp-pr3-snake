using System.Windows;

namespace SnakeGame.App
{
    /// <summary>
    /// View (V у MVVM). Уся логіка у GameViewModel, тут лише ініціалізація вікна.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
