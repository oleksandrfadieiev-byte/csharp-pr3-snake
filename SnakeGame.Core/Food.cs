namespace SnakeGame.Core
{
    public enum FoodKind { Apple, Golden }

    /// <summary>Їжа на полі: де лежить, скільки очок дає і на скільки клітинок подовжує змійку.</summary>
    public sealed record Food(Position Position, FoodKind Kind, int Points, int Growth);

    // Патерн "Фабричний метод": кожна фабрика створює свій вид їжі,
    // а код гри працює з базовим класом і не знає подробиць.

    public abstract class FoodFactory
    {
        public abstract Food Create(Position position);
    }

    public sealed class AppleFactory : FoodFactory
    {
        public override Food Create(Position position) => new Food(position, FoodKind.Apple, Points: 10, Growth: 1);
    }

    public sealed class GoldenAppleFactory : FoodFactory
    {
        public override Food Create(Position position) => new Food(position, FoodKind.Golden, Points: 50, Growth: 3);
    }

    /// <summary>Вибирає фабрику і вільну клітинку. Кожна п'ята їжа золота.</summary>
    public sealed class FoodSpawner
    {
        public const int GoldenEvery = 5;

        private readonly IRandomSource _random;
        private readonly FoodFactory _apple = new AppleFactory();
        private readonly FoodFactory _golden = new GoldenAppleFactory();
        private int _spawned;

        public FoodSpawner(IRandomSource random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public Food? Spawn(int width, int height, Snake snake)
        {
            var free = new List<Position>();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    var p = new Position(x, y);
                    if (!snake.Contains(p)) free.Add(p);
                }
            if (free.Count == 0) return null;

            _spawned++;
            var factory = _spawned % GoldenEvery == 0 ? _golden : _apple;
            return factory.Create(free[_random.Next(free.Count)]);
        }
    }
}
