using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace WpfApp2
{
    public class TetrisGame
    {
        public const int Rows = 20;
        public const int Cols = 10;
        public const int CellSize = 24;

        private static readonly Random Rnd = new Random();

        private static readonly Point[][] FigureShapes =
        {
            new Point[] { new Point(0,0), new Point(1,0), new Point(0,1), new Point(1,1) }, // O
            new Point[] { new Point(0,0), new Point(1,0), new Point(2,0), new Point(3,0) }, // I
            new Point[] { new Point(0,0), new Point(0,1), new Point(1,1), new Point(2,1) }, // J
            new Point[] { new Point(2,0), new Point(0,1), new Point(1,1), new Point(2,1) }, // L
            new Point[] { new Point(1,0), new Point(2,0), new Point(0,1), new Point(1,1) }, // S
            new Point[] { new Point(0,0), new Point(1,0), new Point(1,1), new Point(2,1) }, // Z
            new Point[] { new Point(1,0), new Point(0,1), new Point(1,1), new Point(2,1) }, // T
        };

        private static readonly IBrush[] FigureBrushes =
        {
            Brushes.Yellow,
            Brushes.Cyan,
            new SolidColorBrush(Color.FromRgb(0, 80, 220)),
            new SolidColorBrush(Color.FromRgb(255, 140, 0)),
            Brushes.LimeGreen,
            Brushes.Red,
            Brushes.MediumPurple,
        };

        // null = пусто, IBrush = занято (цвет зафиксированной фигуры)
        public IBrush[,] Field { get; } = new IBrush[Rows, Cols];
        public List<Point> CurrentFigure { get; private set; } = new List<Point>();
        public IBrush FigureBrush { get; private set; } = Brushes.Red;
        public int FigureX { get; private set; }
        public int FigureY { get; private set; }
        public int Score { get; private set; }
        public bool IsGameOver { get; private set; }

        public List<Point> HeldFigure { get; private set; }
        public IBrush HeldBrush { get; private set; }
        public bool CanHold { get; private set; } = true;

        private int _currentIndex = -1;
        private int _heldIndex = -1;

        public event Action GameOver;
        public event Action ScoreChanged;

        public void Start()
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    Field[r, c] = null;
            Score = 0;
            IsGameOver = false;
            HeldFigure = null;
            HeldBrush = null;
            _heldIndex = -1;
            SpawnNewFigure();
        }

        public void SpawnNewFigure()
        {
            SpawnFigureByIndex(Rnd.Next(FigureShapes.Length));
            CanHold = true;
        }

        private void SpawnFigureByIndex(int index)
        {
            _currentIndex = index;
            CurrentFigure = new List<Point>(FigureShapes[index]);
            FigureX = 3;
            FigureY = 0;
            FigureBrush = FigureBrushes[index];

            if (!CanMove(FigureX, FigureY, CurrentFigure))
            {
                IsGameOver = true;
                GameOver?.Invoke();
            }
        }

        public void Hold()
        {
            if (!CanHold || IsGameOver) return;

            int savedIndex = _currentIndex;

            if (_heldIndex < 0)
            {
                _heldIndex = savedIndex;
                HeldFigure = new List<Point>(FigureShapes[_heldIndex]);
                HeldBrush = FigureBrushes[_heldIndex];
                SpawnNewFigure();
            }
            else
            {
                int tempHeld = _heldIndex;
                _heldIndex = savedIndex;
                HeldFigure = new List<Point>(FigureShapes[_heldIndex]);
                HeldBrush = FigureBrushes[_heldIndex];
                SpawnFigureByIndex(tempHeld);
            }

            CanHold = false;
        }

        public bool CanMove(int x, int y, List<Point> figure)
        {
            foreach (var p in figure)
            {
                int nx = x + (int)p.X;
                int ny = y + (int)p.Y;
                if (nx < 0 || nx >= Cols || ny < 0 || ny >= Rows)
                    return false;
                if (Field[ny, nx] != null)
                    return false;
            }
            return true;
        }

        public void MoveLeft()
        {
            if (!IsGameOver && CanMove(FigureX - 1, FigureY, CurrentFigure))
                FigureX--;
        }

        public void MoveRight()
        {
            if (!IsGameOver && CanMove(FigureX + 1, FigureY, CurrentFigure))
                FigureX++;
        }

        public void MoveDown()
        {
            if (IsGameOver) return;

            if (!CanMove(FigureX, FigureY + 1, CurrentFigure))
            {
                PlaceFigure();
                ClearFullLines();
                SpawnNewFigure();
            }
            else
            {
                FigureY++;
            }
        }

        public void HardDrop()
        {
            if (IsGameOver) return;
            while (CanMove(FigureX, FigureY + 1, CurrentFigure))
                FigureY++;
            PlaceFigure();
            ClearFullLines();
            SpawnNewFigure();
        }

        public void RotateFigure()
        {
            if (IsGameOver) return;

            var rotated = new List<Point>();
            foreach (var p in CurrentFigure)
                rotated.Add(new Point(p.Y, -p.X));

            double minX = double.MaxValue, minY = double.MaxValue;
            foreach (var p in rotated)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
            }
            var normalized = new List<Point>();
            foreach (var p in rotated)
                normalized.Add(new Point(p.X - minX, p.Y - minY));

            if (CanMove(FigureX, FigureY, normalized))
                CurrentFigure = normalized;
        }

        private void PlaceFigure()
        {
            foreach (var p in CurrentFigure)
            {
                int fx = FigureX + (int)p.X;
                int fy = FigureY + (int)p.Y;
                if (fy >= 0 && fy < Rows && fx >= 0 && fx < Cols)
                    Field[fy, fx] = FigureBrush;
            }
        }

        private void ClearFullLines()
        {
            int cleared = 0;
            for (int r = Rows - 1; r >= 0; r--)
            {
                bool full = true;
                for (int c = 0; c < Cols; c++)
                    if (Field[r, c] == null) { full = false; break; }

                if (!full) continue;

                for (int row = r; row > 0; row--)
                    for (int col = 0; col < Cols; col++)
                        Field[row, col] = Field[row - 1, col];
                for (int col = 0; col < Cols; col++)
                    Field[0, col] = null;
                r++;
                cleared++;
            }

            if (cleared <= 0) return;

            if      (cleared == 1) Score += 100;
            else if (cleared == 2) Score += 300;
            else if (cleared == 3) Score += 500;
            else                   Score += 800;

            ScoreChanged?.Invoke();
        }
    }
}
