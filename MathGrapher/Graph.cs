using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MathGrapher
{
    public class Graph : Control
    {
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register("XAxis", typeof(AxisDefinition), typeof(Graph), new PropertyMetadata(null, XAxisChanged));
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register("YAxis", typeof(AxisDefinition), typeof(Graph), new PropertyMetadata(null, YAxisChanged));

        private Dictionary<FunctionDefinition, Ellipse> animatedPoints = new Dictionary<FunctionDefinition, Ellipse>();
        private Timer animationTimer;
        private ObservableCollection<FunctionDefinition> functionDefinitions;
        private Canvas graphCanvas;
        private double originTickIndexX;
        private double originTickIndexY;
        private double originX;
        private double originY;
        private int position;
        private ICommand saveAsGifCommand;
        private ICommand saveAsPngCommand;
        private double tickCountX;
        private double tickCountY;
        private double tickWidthX;
        private double tickWidthY;

        static Graph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Graph), new FrameworkPropertyMetadata(typeof(Graph)));
        }

        public Graph()
        {
            functionDefinitions = new ObservableCollection<FunctionDefinition>();
            functionDefinitions.CollectionChanged += FunctionDefinitions_CollectionChanged;
        }

        public GraphAnimation Animation { get; set; }

        public ObservableCollection<FunctionDefinition> FunctionDefinitions
        {
            get
            {
                return functionDefinitions;
            }
        }

        public int Position
        {
            get
            {
                return position;
            }
        }

        public TicksDefinition Ticks { get; set; }

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

            Draw();
        }

        public void Pause()
        {
            animationTimer.Stop();
        }

        public void Seek(int value)
        {
            if (Animation == null || value >= Animation.Values.Count)
            {
                return;
            }

            foreach (var item in animatedPoints)
            {
                var result = item.Key.Function(Animation.Values[value]);
                Canvas.SetLeft(item.Value, originX + (result.X / XAxis.Interval) * tickWidthX - item.Key.Thickness / 2);
                Canvas.SetTop(item.Value, originY - (result.Y / YAxis.Interval) * tickWidthY - item.Key.Thickness / 2);
            }

            Console.Write($"Seek {value}");

            position = value;
        }

        public void Start()
        {
            if (Animation == null)
            {
                return;
            }

            if (animationTimer == null)
            {
                animationTimer = new Timer(Animation.Delay.TotalMilliseconds);
                animationTimer.AutoReset = true;
                animationTimer.Elapsed += AnimationTimer_Elapsed;
            }

            foreach (var functionDefinition in FunctionDefinitions)
            {
                if (!functionDefinition.IsAnimated)
                {
                    continue;
                }

                if (!animatedPoints.ContainsKey(functionDefinition))
                {
                    var point = new Ellipse();
                    point.Fill = functionDefinition.Brush;
                    point.StrokeThickness = 0;
                    point.Width = functionDefinition.Thickness;
                    point.Height = functionDefinition.Thickness;
                    graphCanvas.Children.Add(point);
                    animatedPoints.Add(functionDefinition, point);
                }
            }

            animationTimer.Start();
        }

        public void Stop()
        {
            animationTimer.Stop();
            Seek(0);
        }

        internal void DrawFunctions()
        {
            if (graphCanvas == null || FunctionDefinitions == null)
            {
                return;
            }

            foreach (var functionDefinition in FunctionDefinitions)
            {
                if (!functionDefinition.IsAnimated)
                {
                    var sampleWidth = (XAxis.Max - XAxis.Min) / functionDefinition.SampleCount;

                    var geometry = new StreamGeometry();
                    var path = new Path();
                    path.Data = geometry;
                    path.StrokeThickness = functionDefinition.Thickness;
                    path.Stroke = functionDefinition.Brush;
                    var context = geometry.Open();
                    
                    for (int i = 0; i < functionDefinition.SampleCount; i++)
                    {
                        var result = functionDefinition.Function(XAxis.Min + sampleWidth * i);

                        var x = originX + (result.X / XAxis.Interval) * tickWidthX - functionDefinition.Thickness / 2;
                        var y = originY - (result.Y / YAxis.Interval) * tickWidthY - functionDefinition.Thickness / 2;

                        if (i == 0)
                        {
                            context.BeginFigure(new Point(x, y), false, false);
                        }
                        else
                        {
                            context.LineTo(new Point(x, y), true, true);
                        }
                    }
                   
                    context.Close();

                    graphCanvas.Children.Add(path);
                }
            }
        }

        private static void XAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = (Graph)d;

            graph.Draw();
        }

        private static void YAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graph = (Graph)d;

            graph.Draw();
        }

        private void AnimationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Position < Animation.Values.Count)
            {
                Dispatcher.BeginInvoke(new Action(() => Seek(position += 1)));
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(() => Seek(position = 0)));
            }
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
            DrawFunctions();
            DrawXAxis();
            DrawYAxis();
            UpdateContextMenu();
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
            if (XAxis.TickLabelGenerator == null)
            {
                return;
            }

            for (int i = 0; i < tickCountX / 2; i++)
            {
                var tickLabel = XAxis.TickLabelGenerator.Generate(-i * XAxis.Interval);

                if (tickLabel != null)
                {
                    tickLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Canvas.SetLeft(tickLabel, originY - i * tickWidthX - tickLabel.DesiredSize.Width / 2);
                    Canvas.SetTop(tickLabel, originX + XAxis.TickLabelGenerator.LabelOffset);
                    graphCanvas.Children.Add(tickLabel);
                }

                var tickLabel2 = XAxis.TickLabelGenerator.Generate(i * XAxis.Interval);

                if (tickLabel2 != null)
                {
                    tickLabel2.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Canvas.SetLeft(tickLabel2, originY + i * tickWidthX - tickLabel2.DesiredSize.Width / 2);
                    Canvas.SetTop(tickLabel2, originX + XAxis.TickLabelGenerator.LabelOffset);
                    graphCanvas.Children.Add(tickLabel2);
                }
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
            if (YAxis.TickLabelGenerator == null)
            {
                return;
            }

            for (int i = 0; i < tickCountY / 2; i++)
            {
                var tickLabel = YAxis.TickLabelGenerator.Generate(-i * YAxis.Interval);

                if (tickLabel != null)
                {
                    tickLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Canvas.SetLeft(tickLabel, originX - XAxis.TickLabelGenerator.LabelOffset - tickLabel.DesiredSize.Width);
                    Canvas.SetTop(tickLabel, originY + i * tickWidthY - tickLabel.DesiredSize.Height / 2);
                    graphCanvas.Children.Add(tickLabel);
                }

                var tickLabel2 = YAxis.TickLabelGenerator.Generate(i * YAxis.Interval);

                if (tickLabel2 != null)
                {
                    tickLabel2.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Canvas.SetLeft(tickLabel2, originX - XAxis.TickLabelGenerator.LabelOffset - tickLabel2.DesiredSize.Width);
                    Canvas.SetTop(tickLabel2, originY - i * tickWidthY - tickLabel2.DesiredSize.Height / 2);
                    graphCanvas.Children.Add(tickLabel2);
                }
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

        private void FunctionDefinitions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Draw();
        }

        private void UpdateContextMenu()
        {
            if (ContextMenu == null)
            {
                ContextMenu = new ContextMenu();
            }
            else
            {
                ContextMenu.Items.Clear();
            }

            if (Animation != null)
            {
                if (saveAsGifCommand == null)
                {
                    saveAsGifCommand = new SaveAsGifCommand();
                }

                ContextMenu.Items.Add(new MenuItem() { Header = "Save As Animated GIF…", CommandParameter = this, Command = saveAsGifCommand });
            }
            else
            {
                if (saveAsPngCommand == null)
                {
                    saveAsPngCommand = new SaveAsPngCommand();
                }

                ContextMenu.Items.Add(new MenuItem() { Header = "Save As PNG…", CommandParameter = this, Command = saveAsPngCommand });
            }
        }
    }
}