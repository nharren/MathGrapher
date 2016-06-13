using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MathGrapher
{
    public class Graph : Control
    {
        public static readonly DependencyProperty PointColorProperty = DependencyProperty.Register("PointColor", typeof(Color), typeof(Graph), new PropertyMetadata(Colors.Red));
        public static readonly DependencyProperty PointSizeProperty = DependencyProperty.Register("PointSize", typeof(Size), typeof(Graph), new PropertyMetadata(new Size(5d, 5d)));
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register("XAxis", typeof(AxisDefinition), typeof(Graph), new PropertyMetadata(null, XAxisChanged));
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(Graph), new PropertyMetadata(0d, XPropertyChanged));
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register("YAxis", typeof(AxisDefinition), typeof(Graph), new PropertyMetadata(null, YAxisChanged));

        private Canvas graphCanvas;
        private double originTickIndexX;
        private double originTickIndexY;
        private double originX;
        private double originY;
        private Ellipse point;
        private double tickCountX;
        private double tickCountY;
        private double tickWidthX;
        private double tickWidthY;

        static Graph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Graph), new FrameworkPropertyMetadata(typeof(Graph)));
        }

        public Func<double, Point> Function { get; set; }

        public Color PointColor
        {
            get { return (Color)GetValue(PointColorProperty); }
            set { SetValue(PointColorProperty, value); }
        }

        public Size PointSize
        {
            get { return (Size)GetValue(PointSizeProperty); }
            set { SetValue(PointSizeProperty, value); }
        }

        public TicksDefinition Ticks { get; set; }

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public AxisDefinition XAxis
        {
            get { return (AxisDefinition)GetValue(XAxisProperty); }
            set { SetValue(XAxisProperty, value); }
        }

        public AxisDefinition YAxis
        {
            get { return (AxisDefinition)GetValue(YAxisProperty); }
            set { SetValue(YAxisProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            graphCanvas = (Canvas)Template.FindName("PART_GraphCanvas", this);

            graphCanvas.Children.Clear();
            CalculateOrigin();
            DrawXAxis();
            DrawYAxis();
            DrawFunction();
        }

        internal void DrawFunction()
        {
            if (graphCanvas == null || Function == null)
            {
                return;
            }

            if (point != null)
            {
                graphCanvas.Children.Remove(point);
                point = null;
            }

            point = new Ellipse();
            point.Fill = new SolidColorBrush(PointColor);
            point.StrokeThickness = 0;
            point.Width = PointSize.Width;
            point.Height = PointSize.Height;

            var result = Function(X);

            Canvas.SetLeft(point, originX + (result.X / XAxis.Interval) * tickWidthX - PointSize.Width / 2);
            Canvas.SetTop(point, originY - (result.Y / YAxis.Interval) * tickWidthY - PointSize.Height / 2);

            graphCanvas.Children.Add(point);
        }

        private static void XAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = (Graph)d;

            graph.ApplyTemplate();
        }

        private static void XPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = (Graph)d;

            graph.DrawFunction();
        }

        private static void YAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = (Graph)d;

            graph.ApplyTemplate();
        }

        private void CalculateOrigin()
        {
            tickCountX = (XAxis.Max - XAxis.Min) / XAxis.Interval;
            tickCountY = (YAxis.Max - YAxis.Min) / YAxis.Interval;

            tickWidthX = Width / tickCountX;
            tickWidthY = Height / tickCountY;

            if (XAxis != null && XAxis.Min <= 0 && XAxis.Max >= 0)
            {
                originTickIndexX = (0 - XAxis.Min) / XAxis.Interval;
            }
            else
            {
                originTickIndexX = double.NaN;
            }

            if (YAxis != null && YAxis.Min <= 0 && YAxis.Max >= 0)
            {
                originTickIndexY = (0 - YAxis.Min) / YAxis.Interval;
            }
            else
            {
                originTickIndexY = double.NaN;
            }

            if (double.IsNaN(originTickIndexX))
            {
                originX = double.NaN;
            }
            else
            {
                originX = originTickIndexX * tickWidthX;
            }

            if (double.IsNaN(originTickIndexY))
            {
                originY = double.NaN;
            }
            else
            {
                originY = originTickIndexY * tickWidthY;
            }
        }

        private void DrawXAxis()
        {
            if (double.IsNaN(originY))
            {
                return;
            }

            var xAxis = new Line();
            xAxis.Stroke = new SolidColorBrush(XAxis.Color);
            xAxis.StrokeThickness = XAxis.Thickness;
            xAxis.X1 = 0;
            xAxis.Y1 = originY;
            xAxis.X2 = Width;
            xAxis.Y2 = originY;

            graphCanvas.Children.Add(xAxis);

            if (Ticks == null)
            {
                return;
            }

            for (int i = 1; i < tickCountX; i++)
            {
                var tick = new Line();
                tick.Stroke = new SolidColorBrush(Ticks.Color);
                tick.StrokeThickness = Ticks.Thickness;
                tick.X1 = originY - i * tickWidthX;
                tick.Y1 = originX - Ticks.Width / 2;
                tick.X2 = originY - i * tickWidthX;
                tick.Y2 = originX + Ticks.Width / 2;

                graphCanvas.Children.Add(tick);

                var tick2 = new Line();
                tick2.Stroke = new SolidColorBrush(Ticks.Color);
                tick2.StrokeThickness = Ticks.Thickness;
                tick2.X1 = originY + i * tickWidthX;
                tick2.Y1 = originX - Ticks.Width / 2;
                tick2.X2 = originY + i * tickWidthX;
                tick2.Y2 = originX + Ticks.Width / 2;

                graphCanvas.Children.Add(tick2);
            }
        }

        private void DrawYAxis()
        {
            if (double.IsNaN(originX))
            {
                return;
            }

            var yAxis = new Line();
            yAxis.Stroke = new SolidColorBrush(YAxis.Color);
            yAxis.StrokeThickness = YAxis.Thickness;
            yAxis.X1 = originX;
            yAxis.Y1 = 0;
            yAxis.X2 = originX;
            yAxis.Y2 = Height;

            graphCanvas.Children.Add(yAxis);

            if (Ticks == null)
            {
                return;
            }

            for (int i = 1; i < tickCountY; i++)
            {
                var tick = new Line();
                tick.Stroke = new SolidColorBrush(Ticks.Color);
                tick.StrokeThickness = Ticks.Thickness;
                tick.X1 = originX - Ticks.Width / 2;
                tick.Y1 = originY - i * tickWidthY;
                tick.X2 = originX + Ticks.Width / 2;
                tick.Y2 = originY - i * tickWidthY;

                graphCanvas.Children.Add(tick);

                var tick2 = new Line();
                tick2.Stroke = new SolidColorBrush(Ticks.Color);
                tick2.StrokeThickness = Ticks.Thickness;
                tick2.X1 = originX - Ticks.Width / 2;
                tick2.Y1 = originY + i * tickWidthY;
                tick2.X2 = originX + Ticks.Width / 2;
                tick2.Y2 = originY + i * tickWidthY;

                graphCanvas.Children.Add(tick2);
            }
        }
    }
}