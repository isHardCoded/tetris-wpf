using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp2
{
    public static class TetrisRenderer
    {
        private static readonly Brush EmptyBrush = Brushes.Black;
        private static readonly Brush GridStroke = new SolidColorBrush(Color.FromRgb(35, 35, 35));

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
    }
}
