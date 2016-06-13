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
        private Point origin;
        private Ellipse point;

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

            var xAxis = GenerateXAxis(origin);
            var yAxis = GenerateYAxis(origin);

            if (xAxis != null)
            {
                graphCanvas.Children.Add(xAxis);
            }

            if (yAxis != null)
            {
                graphCanvas.Children.Add(yAxis);
            }

            DrawFunction();
        }

        internal void DrawFunction()
        {
            if (graphCanvas == null)
            {
                return;
            }

            var xTickWidth = Width / (XAxis.Max - XAxis.Min);
            var yTickWidth = Height / (YAxis.Max - YAxis.Min);

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

            Canvas.SetLeft(point, origin.X + result.X * xTickWidth - PointSize.Width / 2);
            Canvas.SetTop(point, origin.Y - result.Y * yTickWidth - PointSize.Height / 2);

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
            var xOriginTick = double.NaN;
            var yOriginTick = double.NaN;

            if (XAxis != null && 0 - XAxis.Min < XAxis.Max - XAxis.Min)
            {
                xOriginTick = 0 - XAxis.Min;
            }

            if (YAxis != null && 0 - YAxis.Min < YAxis.Max - YAxis.Min)
            {
                yOriginTick = 0 - YAxis.Min;
            }

            var xTickWidth = Width / (XAxis.Max - XAxis.Min);
            var yTickWidth = Height / (YAxis.Max - YAxis.Min);

            var xOrigin = double.NaN;
            var yOrigin = double.NaN;

            if (!double.IsNaN(xOriginTick))
            {
                xOrigin = xOriginTick * xTickWidth;
            }

            if (!double.IsNaN(yOriginTick))
            {
                yOrigin = yOriginTick * yTickWidth;
            }

            origin = new Point(xOrigin, yOrigin);
        }

        private Line GenerateXAxis(Point origin)
        {
            if (double.IsNaN(origin.Y))
            {
                return null;
            }

            var xAxis = new Line();
            xAxis.Stroke = new SolidColorBrush(XAxis.Color);
            xAxis.StrokeThickness = XAxis.Thickness;
            xAxis.X1 = 0;
            xAxis.Y1 = origin.Y;
            xAxis.X2 = Width;
            xAxis.Y2 = origin.Y;

            return xAxis;
        }

        private Line GenerateYAxis(Point origin)
        {
            if (double.IsNaN(origin.X))
            {
                return null;
            }

            var yAxis = new Line();
            yAxis.Stroke = new SolidColorBrush(YAxis.Color);
            yAxis.StrokeThickness = YAxis.Thickness;
            yAxis.X1 = origin.X;
            yAxis.Y1 = 0;
            yAxis.X2 = origin.X;
            yAxis.Y2 = Height;

            return yAxis;
        }
    }
}