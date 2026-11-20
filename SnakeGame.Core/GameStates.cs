namespace SnakeGame.Core
{
    // Патерн "Стан": поведінка гри на кнопки і таймер залежить від поточного стану.
    // Замість великого switch по enum кожен стан сам вирішує, що робити.

    public interface IGameState
    {
        string Name { get; }
        bool IsRunning { get; }
        void Start(GameEngine game);
        void TogglePause(GameEngine game);
        void Tick(GameEngine game);
    }

    /// <summary>Гра ще не почалася.</summary>
    public sealed class ReadyState : IGameState
    {
        public string Name => "Готово до старту";
        public bool IsRunning => false;
        public void Start(GameEngine game) => game.SetState(new RunningState());
        public void TogglePause(GameEngine game) { }
        public void Tick(GameEngine game) { }
    }

    /// <summary>Гра йде: кожен тік змійка робить крок.</summary>
    public sealed class RunningState : IGameState
    {
        public string Name => "Гра";
        public bool IsRunning => true;
        public void Start(GameEngine game) { }
        public void TogglePause(GameEngine game) => game.SetState(new PausedState());
        public void Tick(GameEngine game) => game.Step();
    }

    /// <summary>Пауза: тіки ігноруються.</summary>
    public sealed class PausedState : IGameState
    {
        public string Name => "Пауза";
        public bool IsRunning => false;
        public void Start(GameEngine game) => game.SetState(new RunningState());
        public void TogglePause(GameEngine game) => game.SetState(new RunningState());
        public void Tick(GameEngine game) { }
    }

    /// <summary>Гру закінчено. Старт починає нову гру.</summary>
    public sealed class GameOverState : IGameState
    {
        public string Name => "Гру закінчено";
        public bool IsRunning => false;
        public void Start(GameEngine game)
        {
            game.Reset();
            game.SetState(new RunningState());
        }
        public void TogglePause(GameEngine game) { }
        public void Tick(GameEngine game) { }
    }
}
