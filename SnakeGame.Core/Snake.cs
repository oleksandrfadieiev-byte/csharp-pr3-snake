namespace SnakeGame.Core
{
    /// <summary>Змійка: список клітинок від голови до хвоста і напрямок руху.</summary>
    public sealed class Snake
    {
        private readonly LinkedList<Position> _body = new LinkedList<Position>();
        private Direction _pending;

        public Snake(Position head, int length, Direction direction)
        {
            if (length < 1) throw new ArgumentOutOfRangeException(nameof(length));
            Direction = direction;
            _pending = direction;
            var back = direction.Opposite();
            var p = head;
            for (int i = 0; i < length; i++)
            {
                _body.AddLast(p);
                p = p.Step(back);
            }
        }

        public Direction Direction { get; private set; }
        public Position Head => _body.First!.Value;
        public Position Tail => _body.Last!.Value;
        public int Length => _body.Count;
        public IEnumerable<Position> Body => _body;

        /// <summary>Запам'ятовує новий напрямок. Розворот назад ігнорується.</summary>
        public void Turn(Direction direction)
        {
            if (direction != Direction.Opposite())
                _pending = direction;
        }

        /// <summary>Клітинка, куди голова потрапить на наступному кроці.</summary>
        public Position NextHead() => Head.Step(_pending);

        /// <summary>Робить крок. Якщо grow == true, хвіст не прибирається і змійка довшає.</summary>
        public void Move(bool grow)
        {
            Direction = _pending;
            _body.AddFirst(Head.Step(Direction));
            if (!grow) _body.RemoveLast();
        }

        public bool Contains(Position p) => _body.Contains(p);
    }
}
