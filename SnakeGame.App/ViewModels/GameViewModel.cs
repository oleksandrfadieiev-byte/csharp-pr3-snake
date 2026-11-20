using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using SnakeGame.Core;

namespace SnakeGame.App.ViewModels
{
    /// <summary>Одна клітинка, яку View малює на полі.</summary>
    public sealed record CellViewModel(double Left, double Top, string Kind);

    /// <summary>
    /// ViewModel (VM у MVVM): перетворює модель GameEngine на властивості і команди,
    /// до яких прив'язується вікно. Таймер теж тут, бо модель не повинна знати про WPF.
    /// </summary>
    public sealed class GameViewModel : INotifyPropertyChanged
    {
        public const int BoardWidth = 20;
        public const int BoardHeight = 15;
        public const double CellSize = 28;

        private readonly GameEngine _game;
        private readonly DispatcherTimer _timer = new DispatcherTimer();

        public GameViewModel()
        {
            _game = new GameEngine(BoardWidth, BoardHeight, new DefaultRandomSource(), new NormalDifficulty());
            _game.Changed += (_, _) => Refresh();
            _game.GameOver += (_, score) => Message = $"Гру закінчено. Рахунок: {score}. Натисни Enter, щоб почати знову.";
            _game.FoodEaten += (_, points) => Message = points > 10 ? "Золоте яблуко! +50" : "";

            _timer.Tick += (_, _) => _game.Tick();

            StartCommand = new RelayCommand(_ => { Message = ""; _game.Start(); });
            PauseCommand = new RelayCommand(_ => _game.TogglePause());
            TurnCommand = new RelayCommand(p => { if (p is Direction d) _game.Turn(d); });
            DifficultyCommand = new RelayCommand(p => SetDifficulty(p as string));

            Message = "Натисни Enter, щоб почати. Керування: стрілки або WASD, пауза: пробіл.";
            Refresh();
        }

        public ObservableCollection<CellViewModel> Cells { get; } = new ObservableCollection<CellViewModel>();

        public double FieldWidth => BoardWidth * CellSize;
        public double FieldHeight => BoardHeight * CellSize;

        public int Score => _game.Score;
        public int BestScore => _game.BestScore;
        public int Length => _game.Snake.Length;
        public string StateName => _game.State.Name;
        public string DifficultyName => _game.Difficulty.Name;

        private string _message = "";
        public string Message
        {
            get => _message;
            private set { _message = value; OnPropertyChanged(); }
        }

        public ICommand StartCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand TurnCommand { get; }
        public ICommand DifficultyCommand { get; }

        private void SetDifficulty(string? name)
        {
            _game.Difficulty = name switch
            {
                "easy" => new EasyDifficulty(),
                "hard" => new HardDifficulty(),
                _ => new NormalDifficulty()
            };
            Refresh();
        }

        /// <summary>Перебудовує список клітинок і повідомляє View про зміну властивостей.</summary>
        private void Refresh()
        {
            Cells.Clear();
            if (_game.Food is Food food)
                Cells.Add(new CellViewModel(food.Position.X * CellSize, food.Position.Y * CellSize,
                    food.Kind == FoodKind.Golden ? "golden" : "apple"));
            bool head = true;
            foreach (var p in _game.Snake.Body)
            {
                Cells.Add(new CellViewModel(p.X * CellSize, p.Y * CellSize, head ? "head" : "body"));
                head = false;
            }

            _timer.Interval = TimeSpan.FromMilliseconds(_game.TickInterval);
            if (_game.State.IsRunning) _timer.Start(); else _timer.Stop();

            OnPropertyChanged(nameof(Score));
            OnPropertyChanged(nameof(BestScore));
            OnPropertyChanged(nameof(Length));
            OnPropertyChanged(nameof(StateName));
            OnPropertyChanged(nameof(DifficultyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
