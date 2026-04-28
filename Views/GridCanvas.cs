using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using GameOfLife.Models;
using GameOfLife.ViewModels;

namespace GameOfLife.Views
{

    public class GridCanvas : FrameworkElement
    {
        private readonly VisualCollection _visuals;
        private readonly DrawingVisual _backgroundVisual; // siatka
        private readonly DrawingVisual _cellsVisual;      // komorki
        private readonly Brush[] _cellBrushes;
        private Point? _lastMousePoint;

        public static readonly DependencyProperty GridProperty =
            DependencyProperty.Register("Grid", typeof(GridBase), typeof(GridCanvas),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnGridOrSizeChanged));

        public GridBase Grid
        {
            get => (GridBase)GetValue(GridProperty);
            set => SetValue(GridProperty, value);
        }

        public static readonly DependencyProperty RefreshCounterProperty =
            DependencyProperty.Register("RefreshCounter", typeof(long), typeof(GridCanvas),
                new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsRender, OnCellsChanged));

        public long RefreshCounter
        {
            get => (long)GetValue(RefreshCounterProperty);
            set => SetValue(RefreshCounterProperty, value);
        }

        public static readonly DependencyProperty CellSizeProperty =
            DependencyProperty.Register("CellSize", typeof(double), typeof(GridCanvas),
                new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnGridOrSizeChanged));

        public double CellSize
        {
            get => (double)GetValue(CellSizeProperty);
            set => SetValue(CellSizeProperty, value);
        }

        public static readonly DependencyProperty UseCirclesProperty =
            DependencyProperty.Register("UseCircles", typeof(bool), typeof(GridCanvas),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnCellsChanged));

        public bool UseCircles
        {
            get => (bool)GetValue(UseCirclesProperty);
            set => SetValue(UseCirclesProperty, value);
        }

        public GridCanvas()
        {
            _visuals = new VisualCollection(this);
            _backgroundVisual = new DrawingVisual();
            _cellsVisual = new DrawingVisual();
            _visuals.Add(_backgroundVisual);
            _visuals.Add(_cellsVisual);

            _cellBrushes = new Brush[] { 
                Brushes.Transparent, 
                Brushes.Black, 
                Brushes.Blue, 
                Brushes.Red, 
                Brushes.Green, 
                Brushes.Yellow 
            };
            foreach (var b in _cellBrushes) b.Freeze();

            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
        }

        private static void OnGridOrSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var canvas = (GridCanvas)d;
            canvas.RenderBackground();
            canvas.RenderCells();
        }

        private static void OnCellsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GridCanvas)d).RenderCells();
        }

        protected override int VisualChildrenCount => _visuals.Count;
        protected override Visual GetVisualChild(int index) => _visuals[index];

        /// <summary> Obliczanie rozmiaru mapy </summary>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (Grid == null) return new Size(0, 0);
            var vm = DataContext as MainViewModel;
            Topology topology = vm?.SelectedTopology ?? Topology.Square;

            if (topology == Topology.Hexagonal)
            {
                double w = Grid.Width * CellSize + CellSize * 0.5;
                double h = Grid.Height * CellSize * 0.75 + CellSize * 0.25;
                return new Size(w, h);
            }
            if (topology == Topology.Triangular)
            {
                double w = Grid.Width * CellSize * 0.5 + CellSize * 0.5;
                double h = Grid.Height * CellSize;
                return new Size(w, h);
            }
            return new Size(Grid.Width * CellSize, Grid.Height * CellSize);
        }

        private void RenderBackground()
        {
            if (Grid == null) return;
            using (var dc = _backgroundVisual.RenderOpen())
            {
                var vm = DataContext as MainViewModel;
                Topology topology = vm?.SelectedTopology ?? Topology.Square;
                Size visualSize = MeasureOverride(default);
                
                dc.DrawRectangle(Brushes.White, null, new Rect(new Point(0, 0), visualSize));

                if (CellSize > 5)
                {
                    var pen = new Pen(Brushes.LightGray, 0.5);
                    pen.Freeze();

                    if (topology == Topology.Square)
                    {
                        for (int x = 0; x <= Grid.Width; x++)
                            dc.DrawLine(pen, new Point(x * CellSize, 0), new Point(x * CellSize, visualSize.Height));
                        for (int y = 0; y <= Grid.Height; y++)
                            dc.DrawLine(pen, new Point(0, y * CellSize), new Point(visualSize.Width, y * CellSize));
                    }
                    else
                    {
                        for (int y = 0; y < Grid.Height; y++)
                            for (int x = 0; x < Grid.Width; x++)
                                if (topology == Topology.Hexagonal)
                                    DrawHexagon(dc, GetCellPosition(x, y, topology), null, pen);
                                else
                                    DrawTriangle(dc, GetCellPosition(x, y, topology), x, y, null, pen);
                    }
                }
            }
        }

        private void RenderCells()
        {
            if (Grid == null) return;
            using (var dc = _cellsVisual.RenderOpen())
            {
                var vm = DataContext as MainViewModel;
                Topology topology = vm?.SelectedTopology ?? Topology.Square;

                for (int y = 0; y < Grid.Height; y++)
                {
                    for (int x = 0; x < Grid.Width; x++)
                    {
                        int state = Grid.Cells[y * Grid.Width + x];
                        if (state == 0) continue;

                        var brush = state < _cellBrushes.Length ? _cellBrushes[state] : Brushes.Black;
                        Point p = GetCellPosition(x, y, topology);
                        
                        if (UseCircles)
                            dc.DrawEllipse(brush, null, new Point(p.X + CellSize / 2, p.Y + CellSize / 2), CellSize / 2, CellSize / 2);
                        else if (topology == Topology.Hexagonal)
                            DrawHexagon(dc, p, brush);
                        else if (topology == Topology.Triangular)
                            DrawTriangle(dc, p, x, y, brush);
                        else
                            dc.DrawRectangle(brush, null, new Rect(p.X, p.Y, CellSize, CellSize));
                    }
                }
            }
        }

        private Point GetCellPosition(int x, int y, Topology topology)
        {
            if (topology == Topology.Hexagonal)
                return new Point(x * CellSize + ((y % 2 == 0) ? CellSize * 0.5 : 0), y * CellSize * 0.75);
            if (topology == Topology.Triangular)
                return new Point(x * CellSize * 0.5, y * CellSize);
            return new Point(x * CellSize, y * CellSize);
        }

        private void DrawHexagon(DrawingContext dc, Point p, Brush? brush, Pen? pen = null)
        {
            var points = new Point[6];
            for (int i = 0; i < 6; i++)
            {
                double angle = Math.PI / 3 * i + Math.PI / 6;
                points[i] = new Point(p.X + CellSize / 2 + CellSize / 2 * Math.Cos(angle), p.Y + CellSize / 2 + CellSize / 2 * Math.Sin(angle));
            }
            var sg = new StreamGeometry();
            using (var ctx = sg.Open()) { ctx.BeginFigure(points[0], true, true); ctx.PolyLineTo(points, true, true); }
            dc.DrawGeometry(brush, pen, sg);
        }

        private void DrawTriangle(DrawingContext dc, Point p, int x, int y, Brush? brush, Pen? pen = null)
        {
            bool isUpright = (x + y) % 2 == 0;
            var points = new Point[3];
            if (isUpright) { points[0] = new Point(p.X + CellSize / 2, p.Y); points[1] = new Point(p.X + CellSize, p.Y + CellSize); points[2] = new Point(p.X, p.Y + CellSize); }
            else { points[0] = new Point(p.X, p.Y); points[1] = new Point(p.X + CellSize, p.Y); points[2] = new Point(p.X + CellSize / 2, p.Y + CellSize); }
            var sg = new StreamGeometry();
            using (var ctx = sg.Open()) { ctx.BeginFigure(points[0], true, true); ctx.PolyLineTo(points, true, true); }
            dc.DrawGeometry(brush, pen, sg);
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.IsRunning = false;
                _lastMousePoint = e.GetPosition(this);
                HandleMouse(_lastMousePoint.Value);
                CaptureMouse();
            }
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsMouseCaptured && _lastMousePoint.HasValue)
            {
                Point currentPoint = e.GetPosition(this);
                // Interpolacja linii
                double dist = Math.Sqrt(Math.Pow(currentPoint.X - _lastMousePoint.Value.X, 2) + Math.Pow(currentPoint.Y - _lastMousePoint.Value.Y, 2));
                int steps = (int)(dist / (CellSize / 2)) + 1;
                for (int i = 0; i <= steps; i++)
                {
                    double t = (double)i / steps;
                    HandleMouse(new Point(_lastMousePoint.Value.X + (currentPoint.X - _lastMousePoint.Value.X) * t, _lastMousePoint.Value.Y + (currentPoint.Y - _lastMousePoint.Value.Y) * t));
                }
                _lastMousePoint = currentPoint;
            }
        }

        private void OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) { ReleaseMouseCapture(); _lastMousePoint = null; }

        /// Wyznacza współrzędne komórki tam gdzie klikamy do rysowania
        private void HandleMouse(Point p)
        {
            if (DataContext is MainViewModel vm && Grid != null)
            {
                int x, y;
                if (vm.SelectedTopology == Topology.Hexagonal) { y = (int)(p.Y / (CellSize * 0.75)); x = (int)((p.X - ((y % 2 == 0) ? CellSize * 0.5 : 0)) / CellSize); }
                else if (vm.SelectedTopology == Topology.Triangular) { y = (int)(p.Y / CellSize); x = (int)(p.X / (CellSize * 0.5)); }
                else { x = (int)(p.X / CellSize); y = (int)(p.Y / CellSize); }
                
                if (x >= 0 && x < Grid.Width && y >= 0 && y < Grid.Height)
                {
                    // Ustawianie koloru w zależności od modelu
                    int color = 1;
                    if (vm.SelectedColoring == ColoringModel.Immigration) color = (RefreshCounter % 2 == 0 ? 1 : 2);
                    else if (vm.SelectedColoring == ColoringModel.QuadLife) color = (int)(RefreshCounter % 4 + 1);

                    Grid.SetCell(x, y, color);
                    vm.RefreshCounter++;
                }
            }
        }
    }
}
