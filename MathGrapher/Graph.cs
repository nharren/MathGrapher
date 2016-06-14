using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MathGrapher
{
    public class Graph : Control
    {
        public static readonly DependencyProperty AnimationDurationProperty = DependencyProperty.Register("AnimationDuration", typeof(Duration), typeof(Graph), new PropertyMetadata(new Duration(TimeSpan.Zero), AnimationDurationChanged));
        public static readonly DependencyProperty DomainProperty = DependencyProperty.Register("Domain", typeof(Vector), typeof(Graph), new PropertyMetadata(default(Vector), DomainChanged));
        public static readonly DependencyProperty FunctionProperty = DependencyProperty.Register("Function", typeof(Func<double, Point>), typeof(Graph), new PropertyMetadata(null, FunctionChanged));
        public static readonly DependencyProperty PointColorProperty = DependencyProperty.Register("PointColor", typeof(Color), typeof(Graph), new PropertyMetadata(Colors.Red));
        public static readonly DependencyProperty PointSizeProperty = DependencyProperty.Register("PointSize", typeof(Size), typeof(Graph), new PropertyMetadata(new Size(5d, 5d)));
        public static readonly DependencyProperty SamplesProperty = DependencyProperty.Register("Samples", typeof(int), typeof(Graph), new PropertyMetadata(500, SamplesChanged));
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register("XAxis", typeof(AxisDefinition), typeof(Graph), new PropertyMetadata(null, XAxisChanged));
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(Graph), new PropertyMetadata(0d, XPropertyChanged));
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register("YAxis", typeof(AxisDefinition), typeof(Graph), new PropertyMetadata(null, YAxisChanged));

        private Canvas graphCanvas;
        private double originTickIndexX;
        private double originTickIndexY;
        private double originX;
        private double originY;
        private double tickCountX;
        private double tickCountY;
        private double tickWidthX;
        private double tickWidthY;

        static Graph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Graph), new FrameworkPropertyMetadata(typeof(Graph)));
        }

        public Duration AnimationDuration
        {
            get { return (Duration)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        public Vector Domain
        {
            get { return (Vector)GetValue(DomainProperty); }
            set { SetValue(DomainProperty, value); }
        }

        public Func<double, Point> Function
        {
            get { return (Func<double, Point>)GetValue(FunctionProperty); }
            set { SetValue(FunctionProperty, value); }
        }

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

        public int Samples
        {
            get { return (int)GetValue(SamplesProperty); }
            set { SetValue(SamplesProperty, value); }
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

        public void Clear()
        {
            BeginAnimation(XProperty, null);

            var points = graphCanvas.Children.OfType<Ellipse>().ToArray();

            foreach (var point in points)
            {
                graphCanvas.Children.Remove(point);
            }
        }

        public override void OnApplyTemplate()
        {
            graphCanvas = (Canvas)Template.FindName("PART_GraphCanvas", this);
            Draw();
        }

        internal void DrawFunction()
        {
            if (graphCanvas == null || Function == null)
            {
                return;
            }

            Clear();

            if (AnimationDuration != TimeSpan.Zero)
            {
                var point = new Ellipse();
                point.Fill = new SolidColorBrush(PointColor);
                point.StrokeThickness = 0;
                point.Width = PointSize.Width;
                point.Height = PointSize.Height;

                graphCanvas.Children.Add(point);

                var doubleAnimation = new DoubleAnimation(Domain.X, Domain.Y, AnimationDuration);
                doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;

                BeginAnimation(XProperty, doubleAnimation);
            }
            else
            {
                var sampleWidth = (Domain.Y - Domain.X) / Samples;

                for (double i = Domain.X; i < Domain.Y; i += sampleWidth)
                {
                    var point = new Ellipse();
                    point.Fill = new SolidColorBrush(PointColor);
                    point.StrokeThickness = 0;
                    point.Width = PointSize.Width;
                    point.Height = PointSize.Height;

                    var result = Function(i);

                    Canvas.SetLeft(point, originX + (result.X / XAxis.Interval) * tickWidthX - PointSize.Width / 2);
                    Canvas.SetTop(point, originY - (result.Y / YAxis.Interval) * tickWidthY - PointSize.Height / 2);

                    graphCanvas.Children.Add(point);
                }
            }
        }

        private static void AnimationDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = (Graph)d;

            graph.DrawFunction();
        }

        private static void DomainChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = (Graph)d;

            graph.DrawFunction();
        }

        private static void FunctionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = (Graph)d;

            graph.DrawFunction();
        }

        private static void SamplesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = (Graph)d;

            graph.DrawFunction();
        }

        private static void XAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = (Graph)d;

            graph.Draw();
        }

        private static void XPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = (Graph)d;

            graph.UpdatePoint();
        }

        private static void YAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = (Graph)d;

            graph.Draw();
        }

        private void CalculateOrigin()
        {
            tickCountX = (XAxis.Max - XAxis.Min) / XAxis.Interval;
            tickCountY = (YAxis.Max - YAxis.Min) / YAxis.Interval;

            tickWidthX = Width / tickCountX;
            tickWidthY = Height / tickCountY;

            if (XAxis != null && XAxis.Min <= 0 && XAxis.Max >= 0)
            {
                originTickIndexX = -XAxis.Min / XAxis.Interval;
            }
            else
            {
                originTickIndexX = double.NaN;
            }

            if (YAxis != null && YAxis.Min <= 0 && YAxis.Max >= 0)
            {
                originTickIndexY = -YAxis.Min / YAxis.Interval;
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

        private void Draw()
        {
            if (graphCanvas == null)
            {
                return;
            }

            graphCanvas.Children.Clear();
            CalculateOrigin();
            DrawFunction();
            DrawXAxis();
            DrawYAxis();
        }

        private void DrawXAxis()
        {
            if (double.IsNaN(originY))
            {
                return;
            }

            DrawXAxisLine();
            DrawXAxisTicks();
            DrawXAxisTickLabels();
        }

        private void DrawXAxisLine()
        {
            var xAxis = new Line();
            xAxis.Stroke = new SolidColorBrush(XAxis.Color);
            xAxis.StrokeThickness = XAxis.Thickness;
            xAxis.X1 = 0;
            xAxis.Y1 = originY;
            xAxis.X2 = Width;
            xAxis.Y2 = originY;

            graphCanvas.Children.Add(xAxis);
        }

        private void DrawXAxisTickLabels()
        {
            if (XAxis.TickLabels == null)
            {
                return;
            }

            for (int i = XAxis.TickLabels.Frequency; i < tickCountX / 2; i += XAxis.TickLabels.Frequency)
            {
                var tickLabel = new TextBlock();
                tickLabel.Foreground = XAxis.TickLabels.Foreground;
                tickLabel.FontFamily = XAxis.TickLabels.FontFamily;
                tickLabel.FontSize = XAxis.TickLabels.FontSize;
                tickLabel.FontStyle = XAxis.TickLabels.FontStyle;
                tickLabel.FontWeight = XAxis.TickLabels.FontWeight;
                tickLabel.Text = (-i * XAxis.Interval).ToString();

                tickLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(tickLabel, originY - i * tickWidthX - tickLabel.DesiredSize.Width / 2);
                Canvas.SetTop(tickLabel, originX + XAxis.TickLabels.Offset);

                graphCanvas.Children.Add(tickLabel);

                var tickLabel2 = new TextBlock();
                tickLabel2.Foreground = XAxis.TickLabels.Foreground;
                tickLabel2.FontFamily = XAxis.TickLabels.FontFamily;
                tickLabel2.FontSize = XAxis.TickLabels.FontSize;
                tickLabel2.FontStyle = XAxis.TickLabels.FontStyle;
                tickLabel2.FontWeight = XAxis.TickLabels.FontWeight;
                tickLabel2.Text = (i * XAxis.Interval).ToString();

                tickLabel2.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(tickLabel2, originY + i * tickWidthX - tickLabel2.DesiredSize.Width / 2);
                Canvas.SetTop(tickLabel2, originX + XAxis.TickLabels.Offset);

                graphCanvas.Children.Add(tickLabel2);
            }
        }

        private void DrawXAxisTicks()
        {
            if (Ticks == null)
            {
                return;
            }

            for (int i = 1; i < tickCountX / 2; i++)
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

            DrawYAxisLine();
            DrawYAxisTicks();
            DrawYAxisTickLabels();
        }

        private void DrawYAxisLine()
        {
            var yAxis = new Line();
            yAxis.Stroke = new SolidColorBrush(YAxis.Color);
            yAxis.StrokeThickness = YAxis.Thickness;
            yAxis.X1 = originX;
            yAxis.Y1 = 0;
            yAxis.X2 = originX;
            yAxis.Y2 = Height;

            graphCanvas.Children.Add(yAxis);
        }

        private void DrawYAxisTickLabels()
        {
            if (YAxis.TickLabels == null)
            {
                return;
            }

            for (int i = YAxis.TickLabels.Frequency; i < tickCountY / 2; i += YAxis.TickLabels.Frequency)
            {
                var tickLabel = new TextBlock();
                tickLabel.Foreground = YAxis.TickLabels.Foreground;
                tickLabel.FontFamily = YAxis.TickLabels.FontFamily;
                tickLabel.FontSize = YAxis.TickLabels.FontSize;
                tickLabel.FontStyle = YAxis.TickLabels.FontStyle;
                tickLabel.FontWeight = YAxis.TickLabels.FontWeight;
                tickLabel.Text = (-i * YAxis.Interval).ToString();

                tickLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(tickLabel, originX - XAxis.TickLabels.Offset - tickLabel.DesiredSize.Width);
                Canvas.SetTop(tickLabel, originY + i * tickWidthY - tickLabel.DesiredSize.Height / 2);

                graphCanvas.Children.Add(tickLabel);

                var tickLabel2 = new TextBlock();
                tickLabel2.Foreground = YAxis.TickLabels.Foreground;
                tickLabel2.FontFamily = YAxis.TickLabels.FontFamily;
                tickLabel2.FontSize = YAxis.TickLabels.FontSize;
                tickLabel2.FontStyle = YAxis.TickLabels.FontStyle;
                tickLabel2.FontWeight = YAxis.TickLabels.FontWeight;
                tickLabel2.Text = (i * YAxis.Interval).ToString();

                tickLabel2.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(tickLabel2, originX - XAxis.TickLabels.Offset - tickLabel2.DesiredSize.Width);
                Canvas.SetTop(tickLabel2, originY - i * tickWidthY - tickLabel2.DesiredSize.Height / 2);

                graphCanvas.Children.Add(tickLabel2);
            }
        }

        private void DrawYAxisTicks()
        {
            if (Ticks == null)
            {
                return;
            }

            for (int i = 1; i < tickCountY / 2; i++)
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

        private void UpdatePoint()
        {
            var point = graphCanvas.Children.OfType<Ellipse>().First();

            var result = Function(X);

            Canvas.SetLeft(point, originX + (result.X / XAxis.Interval) * tickWidthX - PointSize.Width / 2);
            Canvas.SetTop(point, originY - (result.Y / YAxis.Interval) * tickWidthY - PointSize.Height / 2);
        }
    }
}