using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private readonly TetrisGame _game = new TetrisGame();
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            StartGame();
        }

        private void StartGame()
        {
            _game.GameOver += OnGameOver;
            _game.ScoreChanged += OnScoreChanged;
            _game.Start();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += (s, e) =>
            {
                _game.MoveDown();
                TetrisRenderer.Draw(GameCanvas, _game);
            };
            _timer.Start();

            TetrisRenderer.Draw(GameCanvas, _game);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_game.IsGameOver) return;

            switch (e.Key)
            {
                case Key.Left:  _game.MoveLeft();     break;
                case Key.Right: _game.MoveRight();    break;
                case Key.Up:    _game.RotateFigure(); break;
                case Key.Down:  _game.MoveDown();     break;
                case Key.Space: _game.HardDrop();     break;
            }

            TetrisRenderer.Draw(GameCanvas, _game);
        }

        private void OnGameOver()
        {
            _timer.Stop();
            MessageBox.Show($"Game Over!\nScore: {_game.Score}", "Tetris",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnScoreChanged()
        {
            ScoreText.Text = _game.Score.ToString();
        }
    }
}
