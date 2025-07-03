using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        const int rows = 20;
        const int cols = 10;
        const int cellSize = 20;

        private int figureX = 3;
        private int figureY = 0;

        private List<Point> currentFigure = new List<Point>();
        private Brush figureBrush = Brushes.Red;

        private DispatcherTimer timer;

        private List<Point[]> figures = new List<Point[]>
        {
            new Point[] { new Point(0,0), new Point(1, 0), new Point(0, 1), new Point(1, 1) },
            new Point[] { new Point(0,0), new Point(1, 0), new Point(2, 0), new Point(3, 0) },
            new Point[] { new Point(0,0), new Point(0, 1), new Point(1, 1), new Point(2, 1) },
            new Point[] { new Point(2,0), new Point(0, 1), new Point(1, 1), new Point(2, 1) },
            new Point[] { new Point(1,0), new Point(2, 0), new Point(0, 1), new Point(1, 1) },
            new Point[] { new Point(0,0), new Point(1, 0), new Point(1, 1), new Point(2, 1) },
            new Point[] { new Point(1,0), new Point(0, 1), new Point(1, 1), new Point(2, 1) },
        };

        private bool[,] field = new bool[rows, cols];

        private void SpawnNewFigure()
        {
            var rnd = new Random();
            var shape = figures[rnd.Next(figures.Count)];
            currentFigure = new List<Point>(shape);
            figureX = 3;
            figureY = 0;
            figureBrush = new SolidColorBrush(Color.FromRgb(
                        (byte)rnd.Next(50, 256),
                        (byte)rnd.Next(50, 256),
                        (byte)rnd.Next(50, 256)));

            if(!CanMove(figureX, figureY, currentFigure))
            {
                timer.Stop();
                MessageBox.Show("Game Over!");
            }
        }

        private void Draw()
        {
            GameCanvas.Children.Clear();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Rectangle rect = new Rectangle
                    {
                        Width = cellSize - 1,
                        Height = cellSize - 1,
                        Stroke = Brushes.Gray,
                        StrokeThickness = 0.5,
                        Fill = field[r, c] ? Brushes.Blue : Brushes.White
                    };

                    Canvas.SetLeft(rect, c * cellSize);
                    Canvas.SetTop(rect, r * cellSize);

                    GameCanvas.Children.Add(rect);
                }
            }

            foreach (var p in currentFigure)
            {
                Rectangle rect = new Rectangle
                {
                    Width = cellSize - 1,
                    Height = cellSize - 1,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = figureBrush
                };

                Canvas.SetLeft(rect, (figureX + (int)p.X) * cellSize);
                Canvas.SetTop(rect, (figureY + (int)p.Y) * cellSize);

                GameCanvas.Children.Add(rect);
            }
        }

        private bool CanMove(int x, int y, List<Point> figure)
        {
            foreach (var p in figure) {
                int newX = x + (int)p.X;
                int newY = y + (int)p.Y;

                if (newX < 0 || newX >= cols || newY < 0 || newY >= rows)
                {
                    return false;
                }

                if (field[newY, newX])
                {
                    return false;
                }           
            }
            return true;
        }

        private void MoveDown()
        {
            if (!CanMove(figureX, figureY + 1, currentFigure))
            {
                foreach (var p in currentFigure)
                {
                    int fx = figureX + (int)p.X;
                    int fy = figureY + (int)p.Y;

                    if (fy >= 0 && fy < rows && fx >= 0 && fx < cols)
                    {
                        field[fy, fx] = true;
                    }

                }

                ClearFullLines();
                SpawnNewFigure();
            }

            else
            {
                figureY++;
            }                
            Draw();
        }

        private void ClearFullLines()
        {
            for (int r = rows - 1; r >= 0; r--)
            {
                bool full = true;

                for (int c = 0; c < cols; c++)
                {
                    if (!field[r, c])
                    {
                        full = false;
                        break;
                    }
                }

                if (full)
                {
                    for (int row = r; row > 0; row--)
                    {
                        for (int col = 0; col < cols; col++)
                        {
                            field[row, col] = field[row - 1, col];
                        }
                    }

                    for (int col = 0; col < cols; col++)
                    {
                        field[0, col] = false;         
                    }
                    r++;
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            MoveDown();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    if(CanMove(figureX - 1, figureY, currentFigure))
                    {
                        figureX--;
                    }
                    break;

                case Key.Right:
                    if(CanMove(figureX - 1, figureY, currentFigure))
                    {
                        figureX++;
                    }
                    break;

                case Key.Up:
                    RotateFigure();
                    break;

                case Key.Down:
                    MoveDown();
                    break;
            }

            Draw();
        }

        private void RotateFigure()
        {
            var rotated = new List<Point>();

            foreach (var p in currentFigure)
            {
                rotated.Add(new Point(p.Y, -p.X));
            }

            if (CanMove(figureX, figureY, rotated))
            {
                currentFigure = rotated;
            }
        }

        public void StartNewGame()
        {
            Array.Clear(field, 0, field.Length);
            SpawnNewFigure();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            timer.Start();

            Draw();
        }

        public MainWindow()
        {
            InitializeComponent();
            StartNewGame();
        }
    }
}
