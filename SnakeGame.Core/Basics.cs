namespace SnakeGame.Core
{
    /// <summary>Клітинка на ігровому полі.</summary>
    public readonly record struct Position(int X, int Y)
    {
        public Position Step(Direction direction) => direction switch
        {
            Direction.Up => new Position(X, Y - 1),
            Direction.Down => new Position(X, Y + 1),
            Direction.Left => new Position(X - 1, Y),
            Direction.Right => new Position(X + 1, Y),
            _ => this
        };
    }

    public enum Direction { Up, Down, Left, Right }

    public static class DirectionExtensions
    {
        /// <summary>Протилежний напрямок: змійка не може розвернутися в нього за один хід.</summary>
        public static Direction Opposite(this Direction d) => d switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            _ => Direction.Left
        };
    }

    /// <summary>Джерело випадкових чисел. Інтерфейс потрібен, щоб у тестах підставляти передбачувані значення.</summary>
    public interface IRandomSource
    {
        int Next(int maxExclusive);
    }

    public sealed class DefaultRandomSource : IRandomSource
    {
        private readonly Random _random = new Random();
        public int Next(int maxExclusive) => _random.Next(maxExclusive);
    }
}
