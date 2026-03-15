using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        private const int DasDelayMs = 150;
        private const int ArrMs = 50;

        private readonly TetrisGame _game = new TetrisGame();
        private DispatcherTimer _dropTimer;
        private DispatcherTimer _inputTimer;

        private Key? _heldKey;
        private DateTime _keyHeldSince;
        private DateTime _lastAutoMove;
        private bool _dasActive;

        public MainWindow()
        {
            InitializeComponent();
            _game.GameOver += OnGameOver;
            _game.ScoreChanged += OnScoreChanged;
            StartGame();
        }

        private void StartGame()
        {
            _dropTimer?.Stop();
            _inputTimer?.Stop();
            _heldKey = null;
            _dasActive = false;

            GameOverOverlay.IsVisible = false;
            _game.Start();
            ScoreText.Text = "0";

            // Инициализируем GameView — передаём игру и снэпаем позицию
            GameView.Initialize(_game);

            _dropTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _dropTimer.Tick += (s, e) => { _game.MoveDown(); };
            _dropTimer.Start();

            // Input-таймер ~60fps: двигает DAS/ARR, обновляет анимацию и hold-превью
            _inputTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _inputTimer.Tick += OnInputTick;
            _inputTimer.Start();
        }

        private void OnInputTick(object sender, EventArgs e)
        {
            // DAS / ARR
            if (_heldKey != null && !_game.IsGameOver)
            {
                var now = DateTime.UtcNow;
                if (!_dasActive)
                {
                    if ((now - _keyHeldSince).TotalMilliseconds >= DasDelayMs)
                    {
                        _dasActive = true;
                        _lastAutoMove = now;
                        ApplyHeldKey();
                    }
                }
                else if ((now - _lastAutoMove).TotalMilliseconds >= ArrMs)
                {
                    _lastAutoMove = now;
                    ApplyHeldKey();
                }
            }

            // Плавное обновление позиции + перерисовка игрового поля
            GameView.Tick();

            // Hold-превью обновляем здесь же (дёшево — 4 прямоугольника)
            TetrisRenderer.DrawHold(HoldCanvas, _game);
        }

        private void ApplyHeldKey()
        {
            switch (_heldKey)
            {
                case Key.Left:  _game.MoveLeft();  break;
                case Key.Right: _game.MoveRight(); break;
                case Key.Down:  _game.MoveDown();  break;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_game.IsGameOver)
            {
                if (e.Key == Key.R) StartGame();
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.Down:
                    if (_heldKey != e.Key)
                    {
                        _heldKey = e.Key;
                        _keyHeldSince = DateTime.UtcNow;
                        _dasActive = false;
                        ApplyHeldKey();
                    }
                    e.Handled = true;
                    break;

                case Key.Up:
                    _game.RotateFigure();
                    e.Handled = true;
                    break;

                case Key.Space:
                    _game.HardDrop();
                    GameView.Snap(); // мгновенно, без анимации
                    e.Handled = true;
                    break;

                case Key.C:
                    _game.Hold();
                    GameView.Snap(); // новая фигура — снэпаем
                    e.Handled = true;
                    break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == _heldKey)
            {
                _heldKey = null;
                _dasActive = false;
            }
        }

        private void OnGameOver()
        {
            _dropTimer.Stop();
            _inputTimer.Stop();
            FinalScoreText.Text = $"Score: {_game.Score}";
            GameOverOverlay.IsVisible = true;
        }

        private void OnScoreChanged()
        {
            ScoreText.Text = _game.Score.ToString();
            _dropTimer.Interval = TimeSpan.FromMilliseconds(GetDropInterval());
        }

        private int GetDropInterval()
        {
            int level = _game.Score / 1000;
            return Math.Max(100, (int)(500 * Math.Pow(0.85, level)));
        }
    }
}
