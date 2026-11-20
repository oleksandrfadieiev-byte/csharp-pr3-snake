using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnakeGame.Core;

namespace SnakeGame.Tests
{
    /// <summary>Генератор, що завжди повертає задане число: їжа з'являється в передбачуваному місці.</summary>
    internal sealed class FixedRandom : IRandomSource
    {
        private readonly int _value;
        public FixedRandom(int value) { _value = value; }
        public int Next(int maxExclusive) => Math.Min(_value, maxExclusive - 1);
    }

    [TestClass]
    public class SnakeTests
    {
        // Тест: нова змійка довжиною 3 стоїть у ряд позаду голови.
        [TestMethod]
        public void Constructor_Length3Right_BodyBehindHead()
        {
            // Arrange
            var head = new Position(5, 5);
            // Act
            var snake = new Snake(head, 3, Direction.Right);
            // Assert
            CollectionAssert.AreEqual(new[] { new Position(5, 5), new Position(4, 5), new Position(3, 5) }, snake.Body.ToArray());
        }

        // Тест: довжина менше 1 генерує ArgumentOutOfRangeException.
        [TestMethod]
        public void Constructor_ZeroLength_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var head = new Position(5, 5);
            // Act + Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Snake(head, 0, Direction.Right));
        }

        // Тест: крок без росту зсуває змійку, довжина не змінюється.
        [TestMethod]
        public void Move_NoGrow_ShiftsAndKeepsLength()
        {
            // Arrange
            var snake = new Snake(new Position(5, 5), 3, Direction.Right);
            // Act
            snake.Move(grow: false);
            // Assert
            Assert.AreEqual(new Position(6, 5), snake.Head);
            Assert.AreEqual(3, snake.Length);
        }

        // Тест: крок з ростом подовжує змійку на 1.
        [TestMethod]
        public void Move_Grow_IncreasesLength()
        {
            // Arrange
            var snake = new Snake(new Position(5, 5), 3, Direction.Right);
            // Act
            snake.Move(grow: true);
            // Assert
            Assert.AreEqual(4, snake.Length);
        }

        // Тест: розворот у протилежний бік ігнорується.
        [TestMethod]
        public void Turn_Opposite_IsIgnored()
        {
            // Arrange
            var snake = new Snake(new Position(5, 5), 3, Direction.Right);
            // Act
            snake.Turn(Direction.Left);
            snake.Move(grow: false);
            // Assert
            Assert.AreEqual(Direction.Right, snake.Direction);
            Assert.AreEqual(new Position(6, 5), snake.Head);
        }

        // Тест: поворот убік змінює напрямок на наступному кроці.
        [TestMethod]
        public void Turn_Perpendicular_ChangesDirection()
        {
            // Arrange
            var snake = new Snake(new Position(5, 5), 3, Direction.Right);
            // Act
            snake.Turn(Direction.Up);
            snake.Move(grow: false);
            // Assert
            Assert.AreEqual(new Position(5, 4), snake.Head);
        }
    }

    [TestClass]
    public class FoodTests
    {
        // Тест: фабрика яблук створює їжу на 10 очок, що подовжує на 1.
        [TestMethod]
        public void AppleFactory_Create_Returns10PointsGrowth1()
        {
            // Arrange
            FoodFactory factory = new AppleFactory();
            // Act
            var food = factory.Create(new Position(1, 2));
            // Assert
            Assert.AreEqual(FoodKind.Apple, food.Kind);
            Assert.AreEqual(10, food.Points);
            Assert.AreEqual(1, food.Growth);
        }

        // Тест: фабрика золотих яблук створює їжу на 50 очок, що подовжує на 3.
        [TestMethod]
        public void GoldenAppleFactory_Create_Returns50PointsGrowth3()
        {
            // Arrange
            FoodFactory factory = new GoldenAppleFactory();
            // Act
            var food = factory.Create(new Position(1, 2));
            // Assert
            Assert.AreEqual(FoodKind.Golden, food.Kind);
            Assert.AreEqual(50, food.Points);
            Assert.AreEqual(3, food.Growth);
        }

        // Тест: їжа ніколи не з'являється на змійці.
        [TestMethod]
        public void Spawn_Always_NotOnSnake()
        {
            // Arrange
            var snake = new Snake(new Position(2, 0), 3, Direction.Right);
            var spawner = new FoodSpawner(new FixedRandom(0));
            // Act
            var food = spawner.Spawn(5, 5, snake);
            // Assert
            Assert.IsNotNull(food);
            Assert.IsFalse(snake.Contains(food!.Position));
        }

        // Тест: кожна п'ята їжа золота.
        [TestMethod]
        public void Spawn_FifthTime_ReturnsGolden()
        {
            // Arrange
            var snake = new Snake(new Position(2, 2), 1, Direction.Right);
            var spawner = new FoodSpawner(new FixedRandom(0));
            // Act
            Food? food = null;
            for (int i = 0; i < FoodSpawner.GoldenEvery; i++)
                food = spawner.Spawn(5, 5, snake);
            // Assert
            Assert.AreEqual(FoodKind.Golden, food!.Kind);
        }
    }

    [TestClass]
    public class DifficultyTests
    {
        // Тест: на легкому рівні швидкість не залежить від рахунку.
        [TestMethod]
        public void Easy_AnyScore_Returns200()
        {
            // Arrange
            IDifficulty d = new EasyDifficulty();
            // Act
            int interval = d.TickInterval(1000);
            // Assert
            Assert.AreEqual(200, interval);
        }

        // Тест: на складному рівні гра прискорюється, але не швидше за 50 мс.
        [TestMethod]
        public void Hard_HugeScore_ReturnsMinimum50()
        {
            // Arrange
            IDifficulty d = new HardDifficulty();
            // Act
            int start = d.TickInterval(0);
            int late = d.TickInterval(100000);
            // Assert
            Assert.AreEqual(100, start);
            Assert.AreEqual(50, late);
        }
    }

    [TestClass]
    public class GameEngineTests
    {
        private static GameEngine NewGame() => new GameEngine(10, 10, new FixedRandom(0), new NormalDifficulty());

        // Тест: нова гра чекає старту, рахунок 0, змійка довжиною 3.
        [TestMethod]
        public void Constructor_NewGame_ReadyWithZeroScore()
        {
            // Arrange
            // Act
            var game = NewGame();
            // Assert
            Assert.IsInstanceOfType(game.State, typeof(ReadyState));
            Assert.AreEqual(0, game.Score);
            Assert.AreEqual(GameEngine.StartLength, game.Snake.Length);
        }

        // Тест: поле менше 5x5 генерує ArgumentOutOfRangeException.
        [TestMethod]
        public void Constructor_TooSmallBoard_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var random = new FixedRandom(0);
            // Act + Assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GameEngine(3, 3, random, new EasyDifficulty()));
        }

        // Тест: до старту тік нічого не змінює.
        [TestMethod]
        public void Tick_BeforeStart_DoesNotMove()
        {
            // Arrange
            var game = NewGame();
            var head = game.Snake.Head;
            // Act
            game.Tick();
            // Assert
            Assert.AreEqual(head, game.Snake.Head);
        }

        // Тест: після старту тік зсуває змійку на одну клітинку.
        [TestMethod]
        public void Tick_AfterStart_MovesOneCell()
        {
            // Arrange
            var game = NewGame();
            var head = game.Snake.Head;
            game.Start();
            // Act
            game.Tick();
            // Assert
            Assert.AreEqual(new Position(head.X + 1, head.Y), game.Snake.Head);
        }

        // Тест: на паузі змійка стоїть, повторна пауза продовжує гру.
        [TestMethod]
        public void TogglePause_Twice_PausesThenResumes()
        {
            // Arrange
            var game = NewGame();
            game.Start();
            // Act
            game.TogglePause();
            var head = game.Snake.Head;
            game.Tick();
            bool stoodStill = game.Snake.Head == head;
            game.TogglePause();
            // Assert
            Assert.IsTrue(stoodStill);
            Assert.IsInstanceOfType(game.State, typeof(RunningState));
        }

        // Тест: удар об стіну закінчує гру і викликає подію GameOver.
        [TestMethod]
        public void Tick_HitsWall_GameOver()
        {
            // Arrange
            var game = NewGame();
            int? finalScore = null;
            game.GameOver += (_, s) => finalScore = s;
            game.Start();
            // Act
            for (int i = 0; i < 10; i++) game.Tick();
            // Assert
            Assert.IsInstanceOfType(game.State, typeof(GameOverState));
            Assert.AreEqual(0, finalScore);
        }

        // Тест: зіткнення з власним тілом закінчує гру.
        [TestMethod]
        public void Tick_HitsOwnBody_GameOver()
        {
            // Arrange
            var game = NewGame();
            game.Start();
            // Змійкою довжиною 3 врізатися в себе неможливо, тому подовжуємо її до 6.
            for (int i = 0; i < 3; i++) game.Snake.Move(grow: true);
            // Act
            game.Turn(Direction.Up); game.Tick();
            game.Turn(Direction.Left); game.Tick();
            game.Turn(Direction.Down); game.Tick();
            // Assert
            Assert.IsInstanceOfType(game.State, typeof(GameOverState));
        }

        // Тест: коли голова потрапляє на яблуко, рахунок росте на 10, змійка подовжується.
        [TestMethod]
        public void Tick_EatsApple_ScoreAndLengthGrow()
        {
            // Arrange: з FixedRandom(0) перше яблуко лежить у клітинці (0, 0)
            var game = NewGame();
            game.Start();
            var food = game.Food!.Position;
            // ведемо змійку з (5,5) вгору до рядка 0, потім вліво до стовпця 0
            game.Turn(Direction.Up);
            for (int i = 0; i < 5; i++) game.Tick();
            game.Turn(Direction.Left);
            // Act
            for (int i = 0; i < 5; i++) game.Tick();
            // Assert
            Assert.AreEqual(new Position(0, 0), food);
            Assert.AreEqual(10, game.Score);
            Assert.AreEqual(GameEngine.StartLength + 1, game.Snake.Length);
        }

        // Тест: старт після кінця гри починає нову гру з рахунком 0, рекорд зберігається.
        [TestMethod]
        public void Start_AfterGameOver_ResetsScoreKeepsBest()
        {
            // Arrange
            var game = NewGame();
            game.Start();
            game.Turn(Direction.Up);
            for (int i = 0; i < 5; i++) game.Tick();
            game.Turn(Direction.Left);
            for (int i = 0; i < 5; i++) game.Tick();   // з'їли яблуко, рахунок 10
            for (int i = 0; i < 3; i++) game.Tick();   // врізалися в ліву стіну
            // Act
            game.Start();
            // Assert
            Assert.IsInstanceOfType(game.State, typeof(RunningState));
            Assert.AreEqual(0, game.Score);
            Assert.AreEqual(10, game.BestScore);
        }

        // Тест: поворот до старту ігнорується.
        [TestMethod]
        public void Turn_BeforeStart_IsIgnored()
        {
            // Arrange
            var game = NewGame();
            // Act
            game.Turn(Direction.Up);
            game.Start();
            game.Tick();
            // Assert
            Assert.AreEqual(Direction.Right, game.Snake.Direction);
        }
    }
}
