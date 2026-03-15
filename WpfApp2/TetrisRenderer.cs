using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace WpfApp2
{
    public static class TetrisRenderer
    {
        private static readonly IBrush EmptyBrush = Brushes.Black;
        private static readonly IBrush GridStroke = new SolidColorBrush(Color.FromRgb(35, 35, 35));

        public static void Draw(Canvas canvas, TetrisGame game)
        {
            canvas.Children.Clear();

            // Поле (зафиксированные блоки)
            for (int r = 0; r < TetrisGame.Rows; r++)
            {
                for (int c = 0; c < TetrisGame.Cols; c++)
                {
                    var rect = new Rectangle
                    {
                        Width = TetrisGame.CellSize - 1,
                        Height = TetrisGame.CellSize - 1,
                        Fill = game.Field[r, c] ?? EmptyBrush,
                        Stroke = GridStroke,
                        StrokeThickness = 0.5
                    };
                    Canvas.SetLeft(rect, c * TetrisGame.CellSize);
                    Canvas.SetTop(rect, r * TetrisGame.CellSize);
                    canvas.Children.Add(rect);
                }
            }

            // Текущая падающая фигура
            foreach (var p in game.CurrentFigure)
            {
                var rect = new Rectangle
                {
                    Width = TetrisGame.CellSize - 1,
                    Height = TetrisGame.CellSize - 1,
                    Fill = game.FigureBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(rect, (game.FigureX + (int)p.X) * TetrisGame.CellSize);
                Canvas.SetTop(rect, (game.FigureY + (int)p.Y) * TetrisGame.CellSize);
                canvas.Children.Add(rect);
            }
        }

        public static void DrawHold(Canvas canvas, TetrisGame game)
        {
            canvas.Children.Clear();
            if (game.HeldFigure == null) return;

            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;
            foreach (var p in game.HeldFigure)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }

            double figW = (maxX - minX + 1) * TetrisGame.CellSize;
            double figH = (maxY - minY + 1) * TetrisGame.CellSize;
            double offsetX = (canvas.Width - figW) / 2;
            double offsetY = (canvas.Height - figH) / 2;

            IBrush fill = game.CanHold
                ? game.HeldBrush
                : new SolidColorBrush(Color.FromArgb(120, 100, 100, 100));

            foreach (var p in game.HeldFigure)
            {
                var rect = new Rectangle
                {
                    Width = TetrisGame.CellSize - 1,
                    Height = TetrisGame.CellSize - 1,
                    Fill = fill,
                    Stroke = Brushes.White,
                    StrokeThickness = game.CanHold ? 1 : 0.3
                };
                Canvas.SetLeft(rect, offsetX + (p.X - minX) * TetrisGame.CellSize);
                Canvas.SetTop(rect, offsetY + (p.Y - minY) * TetrisGame.CellSize);
                canvas.Children.Add(rect);
            }
        }
    }
}
