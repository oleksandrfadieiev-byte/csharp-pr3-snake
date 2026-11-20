namespace SnakeGame.Core
{
    /// <summary>
    /// Модель гри (M у MVVM): поле, змійка, їжа, рахунок і стан.
    /// Не залежить від WPF, тому її можна повністю перевірити тестами.
    /// </summary>
    public sealed class GameEngine
    {
        public const int StartLength = 3;

        private readonly FoodSpawner _spawner;

        public GameEngine(int width, int height, IRandomSource random, IDifficulty difficulty)
        {
            if (width < 5 || height < 5) throw new ArgumentOutOfRangeException(nameof(width), "Поле має бути щонайменше 5x5.");
            Width = width;
            Height = height;
            _spawner = new FoodSpawner(random);
            Difficulty = difficulty ?? throw new ArgumentNullException(nameof(difficulty));
            State = new ReadyState();
            Snake = null!;
            Reset();
        }

        public int Width { get; }
        public int Height { get; }
        public Snake Snake { get; private set; }
        public Food? Food { get; private set; }
        public int Score { get; private set; }
        public int BestScore { get; private set; }
        public IGameState State { get; private set; }
        public IDifficulty Difficulty { get; set; }

        /// <summary>Скільки ще кроків змійка буде рости після золотого яблука.</summary>
        public int PendingGrowth { get; private set; }

        public int TickInterval => Difficulty.TickInterval(Score);

        // Патерн "Спостерігач": ViewModel підписується на ці події і оновлює екран.
        public event EventHandler? Changed;
        public event EventHandler<int>? FoodEaten;
        public event EventHandler<int>? GameOver;

        public void Start() => State.Start(this);
        public void TogglePause() => State.TogglePause(this);
        public void Tick() => State.Tick(this);

        public void Turn(Direction direction)
        {
            if (State.IsRunning) Snake.Turn(direction);
        }

        /// <summary>Повертає гру до початку: змійка в центрі, рахунок 0.</summary>
        public void Reset()
        {
            Snake = new Snake(new Position(Width / 2, Height / 2), StartLength, Direction.Right);
            Score = 0;
            PendingGrowth = 0;
            Food = _spawner.Spawn(Width, Height, Snake);
            Changed?.Invoke(this, EventArgs.Empty);
        }

        internal void SetState(IGameState state)
        {
            State = state;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Один крок гри. Викликається станом RunningState.</summary>
        internal void Step()
        {
            var next = Snake.NextHead();
            bool eats = Food != null && next == Food.Position;
            bool grow = eats || PendingGrowth > 0;

            // Хвіст звільняє клітинку, якщо змійка не росте, тому в нього можна "наступити".
            bool hitsSelf = Snake.Contains(next) && !(next == Snake.Tail && !grow);
            bool hitsWall = next.X < 0 || next.Y < 0 || next.X >= Width || next.Y >= Height;
            if (hitsWall || hitsSelf)
            {
                BestScore = Math.Max(BestScore, Score);
                SetState(new GameOverState());
                GameOver?.Invoke(this, Score);
                return;
            }

            if (PendingGrowth > 0 && !eats) PendingGrowth--;
            Snake.Move(grow);

            if (eats)
            {
                Score += Food!.Points;
                PendingGrowth += Food.Growth - 1;
                FoodEaten?.Invoke(this, Food.Points);
                Food = _spawner.Spawn(Width, Height, Snake);
            }
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
