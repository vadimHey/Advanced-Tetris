using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Tetris
{
    public partial class PlayWindow : Window
    {
        //Blocks tile colors by their id 
        private readonly ImageSource[] tileImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/TileEmpty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileCyan.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileBlue.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileOrange.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileYellow.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileGreen.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TilePurple.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileRed.png", UriKind.Relative))
        };

        private readonly ImageSource[] blockImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/Block-Empty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-I.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-J.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-L.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-O.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-S.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-T.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-Z.png", UriKind.Relative))
        };

        private readonly Image[,] imageControls;
        private readonly int maxDelay = 1000;
        private readonly int minDelay = 75;
        private readonly int delayDecrease = 25;
        private bool isPaused = false;  
        private bool isGameRunning = false;
        private DateTime lastDropTime;
        private int remainingDelay;

        private GameState gameState = new GameState();
        private ScoreEntry scoreEntry = new ScoreEntry();

        public PlayWindow()
        {
            InitializeComponent();
            imageControls = SetupGameCanvas(gameState.GameGrid);
        }

        private Image[,] SetupGameCanvas(GameGrid grid)
        {
            Image[,] imageControls = new Image[grid.Rows, grid.Columns];
            int cellSize = 25;

            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    Image imageControl = new Image
                    {
                        Width = cellSize,
                        Height = cellSize,
                    };

                    Canvas.SetTop(imageControl, (r - 2) * cellSize + 10);
                    Canvas.SetLeft(imageControl, c * cellSize);
                    GameCanvas.Children.Add(imageControl);
                    imageControls[r, c] = imageControl;
                }
            }

            return imageControls;
        }

        private void DrawGrid(GameGrid grid)
        {
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    int id = grid[r, c];
                    imageControls[r, c].Opacity = 1;
                    imageControls[r, c].Source = tileImages[id];
                }
            }
        }

        private void DrawBlock(Block block)
        {
            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row, p.Column].Opacity = 1;
                imageControls[p.Row, p.Column].Source = tileImages[block.Id];
            }
        }

        private void DrawNextBlock(BlockQueue blockQueue)
        {
            Block next = blockQueue.NextBlock;
            NextImage.Source = blockImages[next.Id];
        }

        private void DrawHeldBlock(Block heldBlock)
        {
            if (heldBlock == null)
            {
                HoldImage.Source = blockImages[0];
            }
            else
            {
                HoldImage.Source = blockImages[heldBlock.Id];
            }
        }

        private void DrawGhostBlock(Block block)
        {
            int dropDistance = gameState.BlockDropDistance();

            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row + dropDistance, p.Column].Opacity = 0.25;
                imageControls[p.Row + dropDistance, p.Column].Source = tileImages[block.Id];
            }
        }

        private void Draw(GameState gameState)
        {
            DrawGrid(gameState.GameGrid);
            DrawGhostBlock(gameState.CurrentBlock);
            DrawBlock(gameState.CurrentBlock);
            DrawNextBlock(gameState.BlockQueue);
            DrawHeldBlock(gameState.HeldBlock);
            ScoreText.Text = $"Счет: {gameState.Score}";
        }

        private async Task GameLoop()
        {
            isGameRunning = true;
            Draw(gameState);

            lastDropTime = DateTime.Now;
            int dropDelay = GetDropDelay();

            while (!gameState.GameOver && isGameRunning)
            {
                if (isPaused)
                {
                    remainingDelay = dropDelay - (int)(DateTime.Now - lastDropTime).TotalMilliseconds;
                    remainingDelay = Math.Max(remainingDelay, 0);
                    await WaitWhilePaused();
                    lastDropTime = DateTime.Now;
                    continue;
                }

                int elapsed = (int)(DateTime.Now - lastDropTime).TotalMilliseconds;

                if (elapsed >= (remainingDelay > 0 ? remainingDelay : dropDelay))
                {
                    gameState.MoveBlockDown();
                    Draw(gameState);

                    lastDropTime = DateTime.Now;
                    dropDelay = GetDropDelay();
                    remainingDelay = 0;
                }

                await Task.Delay(10);
            }

            GameOverMenu.Visibility = Visibility.Visible;
            FinalScoreText.Text = $"Счет: {gameState.Score}";
            scoreEntry.SaveScore(gameState.Score);
        }

        private async Task WaitWhilePaused()
        {
            while (isPaused)
            {
                await Task.Delay(100);
            }
        }

        private int GetDropDelay()
        {
            return Math.Max(minDelay, maxDelay - (gameState.Score * delayDecrease));
        }

        private async void TogglePause()
        {
            if (isPaused)
            {
                isPaused = false;
                PauseOverlay.Visibility = Visibility.Hidden;
                await GameLoop();
            }
            else
            {
                isPaused = true;
                PauseOverlay.Visibility = Visibility.Visible;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver)
            {
                return;
            }

            if (e.Key == Key.Escape)
            {
                TogglePause();
            }
            else if (!isPaused)
            {
                switch (e.Key)
                {
                    case Key.A:
                        gameState.MoveBlockLeft();
                        break;
                    case Key.D:
                        gameState.MoveBlockRight();
                        break;
                    case Key.S:
                        gameState.MoveBlockDown();
                        break;
                    case Key.W:
                        gameState.RotateBlockCW();
                        break;
                    case Key.E:
                        gameState.RotateBlockCCW();
                        break;
                    case Key.C:
                        gameState.HoldBlock();
                        break;
                    case Key.Space:
                        gameState.DropBlock();
                        break;
                    default:
                        return;
                }

                Draw(gameState);
            }
        }

        private async void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            await GameLoop();
        }

        private async void PlayAgain_Click(object semder, RoutedEventArgs e)
        {
            gameState = new GameState();
            GameOverMenu.Visibility = Visibility.Hidden;
            await GameLoop();
        }
    }
}