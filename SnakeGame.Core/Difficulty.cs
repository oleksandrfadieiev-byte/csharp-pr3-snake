namespace SnakeGame.Core
{
    // Патерн "Стратегія": складність визначає швидкість гри.
    // Гра викликає TickInterval і не знає, яка саме стратегія вибрана.

    public interface IDifficulty
    {
        string Name { get; }

        /// <summary>Пауза між кроками в мілісекундах для поточного рахунку.</summary>
        int TickInterval(int score);
    }

    public sealed class EasyDifficulty : IDifficulty
    {
        public string Name => "Легко";
        public int TickInterval(int score) => 200;
    }

    public sealed class NormalDifficulty : IDifficulty
    {
        public string Name => "Нормально";
        public int TickInterval(int score) => Math.Max(80, 150 - score / 50 * 10);
    }

    public sealed class HardDifficulty : IDifficulty
    {
        public string Name => "Складно";
        public int TickInterval(int score) => Math.Max(50, 100 - score / 50 * 10);
    }
}
