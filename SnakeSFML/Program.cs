using SFML.Graphics;
using SFML.Window;
using SFML.System;

namespace Snake
{
    enum Sprites { Ground, Strawberry, Worm, WormHead };
    enum Direction { Up, Left, Down, Right };
    class Game
    {
        private Dictionary<Sprites, Sprite> sprites;
        private Dictionary<Direction, Vector2i> directionDict;
        private int[,] map;
        private Vector2i snakePosition;
        private int snakeLength, tileDimension;
        private Direction snakeDirection;
        Random rng;
        public bool GameLost { get; private set; }

        public Game(int width, int height, int tileDimension)
        {
            this.tileDimension = tileDimension;
            rng = new Random();
            map = new int[height, width]; // 0: nothing     x > 0 : worm part with x lifetime     -1 : strawberry 
            InitiateDictionnary();
            InitiateSnake();
            SpawnStrawberry();
            GameLost = false;
        }
        public void Display(RenderWindow window)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    int tile = map[i, j];
                    Sprite spriteToDisplay;
                    if (tile == 0)
                    {
                        spriteToDisplay = sprites[Sprites.Ground];
                    }
                    else if (tile > 0 && tile < snakeLength)
                    {
                        spriteToDisplay = sprites[Sprites.Worm];
                    }
                    else if (tile == snakeLength)
                    {
                        spriteToDisplay = sprites[Sprites.WormHead];
                    }
                    else if (tile == -1)
                    {
                        spriteToDisplay = sprites[Sprites.Strawberry];
                    }
                    else
                    {
                        throw new Exception("tried to render tile " + tile);
                    }
                    spriteToDisplay.Position = new Vector2f(j * tileDimension, i * tileDimension);
                    window.Draw(spriteToDisplay);                   
                }
            }
        }
        private void InitiateDictionnary()
        {
            sprites = new Dictionary<Sprites, Sprite>();
            sprites.Add(Sprites.Ground, new Sprite(new Texture("sprites/ground.png")));
            sprites.Add(Sprites.Worm, new Sprite(new Texture("sprites/worm.png")));
            sprites.Add(Sprites.WormHead, new Sprite(new Texture("sprites/worm-head.png")));
            sprites.Add(Sprites.Strawberry, new Sprite(new Texture("sprites/strawberry.png")));
            Vector2f scale = new Vector2f(4, 4);
            foreach (Sprite sprite in sprites.Values)
            {
                sprite.Scale = scale;
            }
            directionDict = new Dictionary<Direction, Vector2i>();
            directionDict.Add(Direction.Up, new Vector2i(0, -1));
            directionDict.Add(Direction.Down, new Vector2i(0, 1));
            directionDict.Add(Direction.Left, new Vector2i(-1, 0));
            directionDict.Add(Direction.Right, new Vector2i(1, 0));
        }
        private void InitiateSnake()
        {
            snakeDirection = Direction.Down;
            snakeLength = 3;
            snakePosition = new Vector2i(map.GetLength(1) / 2, snakeLength);
            for (int i = snakeLength; i > 0; i--)
            {
                map[i, snakePosition.X] = i;
            }
        }
        private void SpawnStrawberry()
        {
            while (true)
            {
                int x = rng.Next(0, map.GetLength(1));
                int y = rng.Next(0, map.GetLength(0));
                if (map[y, x] == 0)
                {
                    map[y, x] = -1;
                    break;
                }
            }
        }
        public void Update(ref double fps)
        {
            UpdateDirection();
            Vector2i nextTile = snakePosition + directionDict[snakeDirection];
            if (Keyboard.IsKeyPressed(Keyboard.Key.D))
            {
                Console.WriteLine($"current X: ");
                Console.WriteLine($"X: {nextTile.X}");
            }
            if (nextTile.X < 0 || nextTile.X >= map.GetLength(1) || nextTile.Y < 0 || nextTile.Y >= map.GetLength(0))
            {
                GameLost = true;
            }
            else
            {
                int nextTileContent = map[nextTile.Y, nextTile.X];
                if (nextTileContent == 0)
                {
                    DecaySnake();
                    map[nextTile.Y, nextTile.X] = snakeLength;
                    snakePosition = nextTile;
                }
                else if (nextTileContent == -1)
                {
                    snakeLength++;
                    map[nextTile.Y, nextTile.X] = snakeLength;
                    snakePosition = nextTile;
                    SpawnStrawberry();
                    fps += 0.2;
                }
                else
                {
                    GameLost = true;
                }
            }
        }
        private void DecaySnake()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j] > 0)
                    {
                        map[i, j] = map[i, j] - 1;
                    }
                }
            }
        }
        private void UpdateDirection()
        {
            Direction newDirection = snakeDirection;
            if (Keyboard.IsKeyPressed(Keyboard.Key.Up)) { newDirection = Direction.Up; }
            else if (Keyboard.IsKeyPressed(Keyboard.Key.Left)) { newDirection = Direction.Left; }
            else if (Keyboard.IsKeyPressed(Keyboard.Key.Down)) { newDirection = Direction.Down; }
            else if (Keyboard.IsKeyPressed(Keyboard.Key.Right)) { newDirection = Direction.Right; }
            if ((int)snakeDirection != ((int)newDirection + 2) % 4)
            {
                snakeDirection = newDirection;
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            double fps = 5;
            const int TileDimension = 64;
            const int Width = 20;
            const int Height = 12;
            RenderWindow window = new RenderWindow(new VideoMode(Width * TileDimension, Height * TileDimension), "Snake");
            Game game = new Game(Width, Height, TileDimension);
           
            while (!Keyboard.IsKeyPressed(Keyboard.Key.Escape) && !game.GameLost)
            {
                window.Clear();
                window.DispatchEvents();
                game.Update(ref fps);
                game.Display(window);
                window.Display();
                System.Threading.Thread.Sleep((int)(1000 / fps));
            }
        }
    }
}