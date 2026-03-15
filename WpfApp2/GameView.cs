using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace WpfApp2
{
    /// <summary>
    /// Кастомный контрол с плавным движением фигуры через интерполяцию позиции.
    /// Рендерит поле через DrawingContext — без создания сотен Rectangle-объектов.
    /// </summary>
    public class GameView : Control
    {
        // Скорость визуального движения: 24px за ~35ms (≈686 px/s)
        private const double VisualSpeed = 686.0;

        private TetrisGame _game;
        private double _visualX;
        private double _visualY;
        private DateTime _lastTick = DateTime.UtcNow;

        private IBrush _emptyBrush;
        private IBrush _ghostBrush;
        private IPen _gridPen;
        private IPen _whitePen;

        public void Initialize(TetrisGame game)
        {
            _game = game;

            _emptyBrush = Brushes.Black;
            _ghostBrush = new SolidColorBrush(Color.FromArgb(55, 255, 255, 255));
            _gridPen    = new Pen(new SolidColorBrush(Color.FromRgb(35, 35, 35)), 0.5);
            _whitePen   = new Pen(Brushes.White, 1.0);

            Snap();
        }

        /// <summary>Мгновенно ставит визуальную позицию на логическую (без анимации).</summary>
        public void Snap()
        {
            _visualX = _game.FigureX;
            _visualY = _game.FigureY;
            _lastTick = DateTime.UtcNow;
        }

        /// <summary>Вызывается каждые ~16ms из input-таймера. Двигает визуальную позицию к логической.</summary>
        public void Tick()
        {
            if (_game == null) return;

            var now = DateTime.UtcNow;
            double dt = (now - _lastTick).TotalMilliseconds;
            _lastTick = now;

            // Новая фигура заспавнилась (Y резко прыгнул вверх) — снэпаем мгновенно
            if (_game.FigureY < _visualY - 1.0)
            {
                _visualX = _game.FigureX;
                _visualY = _game.FigureY;
            }
            else
            {
                double maxDelta = VisualSpeed * dt / 1000.0;
                _visualX = MoveToward(_visualX, _game.FigureX, maxDelta);
                _visualY = MoveToward(_visualY, _game.FigureY, maxDelta);
            }

            InvalidateVisual();
        }

        public override void Render(DrawingContext ctx)
        {
            if (_game == null) return;

            int cs = TetrisGame.CellSize;

            // Фон
            ctx.FillRectangle(Brushes.Black, new Rect(0, 0, Bounds.Width, Bounds.Height));

            // Зафиксированные блоки + сетка
            for (int r = 0; r < TetrisGame.Rows; r++)
            {
                for (int c = 0; c < TetrisGame.Cols; c++)
                {
                    var fill = _game.Field[r, c] ?? _emptyBrush;
                    ctx.DrawRectangle(fill, _gridPen,
                        new Rect(c * cs, r * cs, cs - 1, cs - 1));
                }
            }

            // Ghost piece — тень куда упадёт фигура
            int ghostY = _game.FigureY;
            while (_game.CanMove(_game.FigureX, ghostY + 1, _game.CurrentFigure))
                ghostY++;

            if (ghostY != _game.FigureY)
            {
                foreach (var p in _game.CurrentFigure)
                {
                    ctx.DrawRectangle(_ghostBrush, null,
                        new Rect(
                            (_game.FigureX + p.X) * cs,
                            (ghostY + p.Y) * cs,
                            cs - 1, cs - 1));
                }
            }

            // Текущая фигура — плавная интерполированная позиция
            foreach (var p in _game.CurrentFigure)
            {
                ctx.DrawRectangle(_game.FigureBrush, _whitePen,
                    new Rect(
                        (_visualX + p.X) * cs,
                        (_visualY + p.Y) * cs,
                        cs - 1, cs - 1));
            }
        }

        private static double MoveToward(double current, double target, double maxDelta)
        {
            double diff = target - current;
            if (Math.Abs(diff) <= maxDelta) return target;
            return current + Math.Sign(diff) * maxDelta;
        }
    }
}
