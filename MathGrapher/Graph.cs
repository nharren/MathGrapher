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
using System.Linq;

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
        private Point origin;
        private int position;
        private ICommand saveAsGifCommand;
        private ICommand saveAsPngCommand;
        private double xAxisTickWidth;
        private double yAxisTickWidth;
        private ObservableCollection<LegendDefinition> legendDefinitions;

        static Graph()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Graph), new FrameworkPropertyMetadata(typeof(Graph)));
        }

        public Graph()
        {
            functionDefinitions = new ObservableCollection<FunctionDefinition>();
            functionDefinitions.CollectionChanged += FunctionDefinitions_CollectionChanged;

            legendDefinitions = new ObservableCollection<LegendDefinition>();
            legendDefinitions.CollectionChanged += LegendDefinitions_CollectionChanged;
        }

        private void LegendDefinitions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            DrawLegend();
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

                Canvas.SetLeft(item.Value, origin.X + (result.X / XAxis.Interval) * xAxisTickWidth - item.Key.Thickness / 2);
                Canvas.SetTop(item.Value, origin.Y - (result.Y / YAxis.Interval) * yAxisTickWidth - item.Key.Thickness / 2);
            }

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

        public ObservableCollection<LegendDefinition> LegendDefinitions
        {
            get
            {
                return legendDefinitions;
            }
        }

        internal void DrawLegend()
        {
            if (legendDefinitions.Count == 0 || graphCanvas == null)
            {
                return;
            }

            var legendGrid = graphCanvas.Children.OfType<Grid>().FirstOrDefault(g => g.Name == "legendGrid");

            if (legendGrid != null)
            {
                graphCanvas.Children.Remove(legendGrid);
            }

            legendGrid = new Grid();
            legendGrid.Name = "legendGrid";
            legendGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            legendGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            for (int i = 0; i < LegendDefinitions.Count; i++)
            {
                legendGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var brushRectangle = new Rectangle();
                brushRectangle.Height = 10.0;
                brushRectangle.Width = 10.0;
                brushRectangle.StrokeThickness = 0.0;
                brushRectangle.Fill = LegendDefinitions[i].Brush;

                Grid.SetRow(brushRectangle, i);

                legendGrid.Children.Add(brushRectangle);

                var descriptionTextBlock = new TextBlock();
                descriptionTextBlock.FontFamily = new FontFamily("Consolas");
                descriptionTextBlock.FontSize = 14.666;
                descriptionTextBlock.Text = " = " + LegendDefinitions[i].Description;

                Grid.SetRow(descriptionTextBlock, i);
                Grid.SetColumn(descriptionTextBlock, 1);

                legendGrid.Children.Add(descriptionTextBlock);
            }

            Canvas.SetBottom(legendGrid, 10);
            Canvas.SetRight(legendGrid, 10);

            graphCanvas.Children.Add(legendGrid);
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
                    if (functionDefinition.ConnectSamples)
                    {
                        DrawConnectedFunction(functionDefinition);
                    }
                    else
                    {
                        DrawUnconnectedFunction(functionDefinition);
                    }
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
            else if (Animation.Repeat)
            {
                Dispatcher.BeginInvoke(new Action(() => Seek(position = 0)));
            }
        }

        private void CalculateOrigin()
        {
            xAxisTickWidth = Width / XAxis.TickCount;
            yAxisTickWidth = Height / YAxis.TickCount;

            var originTickIndexX = double.NaN;
            var originTickIndexY = double.NaN;

            if (XAxis != null && XAxis.Min <= 0 && XAxis.Max >= 0)
            {
                originTickIndexX = -XAxis.Min / XAxis.Interval;
            }

            if (YAxis != null && YAxis.Min <= 0 && YAxis.Max >= 0)
            {
                originTickIndexY = -YAxis.Min / YAxis.Interval;
            }

            var originX = double.NaN;
            var originY = double.NaN;

            if (!double.IsNaN(originTickIndexX))
            {
                originX = originTickIndexX * xAxisTickWidth;
            }

            if (!double.IsNaN(originTickIndexY))
            {
                originY = originTickIndexY * yAxisTickWidth;
            }

            origin = new Point(originX, originY);
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
            DrawLegend();
            UpdateContextMenu();
        }

        private void DrawConnectedFunction(FunctionDefinition functionDefinition)
        {
            var sampleInterval = (XAxis.Max - XAxis.Min) / functionDefinition.SampleCount;

            var streamGeometry = new StreamGeometry();

            using (var streamGeometryContext = streamGeometry.Open())
            {
                for (int i = 0; i <= functionDefinition.SampleCount; i++)
                {
                    var result = functionDefinition.Function(XAxis.Min + sampleInterval * i);

                    var x = origin.X + (result.X / XAxis.Interval) * xAxisTickWidth - functionDefinition.Thickness / 2;
                    var y = origin.Y - (result.Y / YAxis.Interval) * yAxisTickWidth - functionDefinition.Thickness / 2;

                    if (i == 0)
                    {
                        streamGeometryContext.BeginFigure(new Point(x, y), false, false);
                    }
                    else
                    {
                        streamGeometryContext.LineTo(new Point(x, y), true, true);
                    }
                }
            }

            var path = new Path();
            path.Data = streamGeometry;
            path.StrokeThickness = functionDefinition.Thickness;
            path.Stroke = functionDefinition.Brush;

            graphCanvas.Children.Add(path);
        }

        private void DrawUnconnectedFunction(FunctionDefinition functionDefinition)
        {
            var sampleInterval = (XAxis.Max - XAxis.Min) / functionDefinition.SampleCount;

            for (int i = 0; i <= functionDefinition.SampleCount; i++)
            {
                var result = functionDefinition.Function(XAxis.Min + sampleInterval * i);

                if (double.IsNaN(result.X) || double.IsNaN(result.Y))
                {
                    continue;
                }

                var x = origin.X + (result.X / XAxis.Interval) * xAxisTickWidth - functionDefinition.Thickness / 2;
                var y = origin.Y - (result.Y / YAxis.Interval) * yAxisTickWidth - functionDefinition.Thickness / 2;

                var point = new Ellipse();
                point.Width = functionDefinition.Thickness;
                point.Height = functionDefinition.Thickness;
                point.Fill = functionDefinition.Brush;
                point.StrokeThickness = 0.0;

                Canvas.SetLeft(point, x);
                Canvas.SetTop(point, y);

                graphCanvas.Children.Add(point);
            }
        }

        private void DrawXAxis()
        {
            if (double.IsNaN(origin.Y))
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
            xAxis.Y1 = origin.Y;
            xAxis.X2 = Width;
            xAxis.Y2 = origin.Y;

            graphCanvas.Children.Add(xAxis);
        }

        private void DrawXAxisTickLabels()
        {
            if (XAxis.TickLabelGenerator == null)
            {
                return;
            }

            for (int i = 0; i < XAxis.TickCount / 2; i++)
            {
                var tickLabel = XAxis.TickLabelGenerator.Generate(-i * XAxis.Interval);

                if (tickLabel != null)
                {
                    tickLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Canvas.SetLeft(tickLabel, origin.Y - i * xAxisTickWidth - tickLabel.DesiredSize.Width / 2);
                    Canvas.SetTop(tickLabel, origin.X + XAxis.TickLabelGenerator.LabelOffset);
                    graphCanvas.Children.Add(tickLabel);
                }

                var tickLabel2 = XAxis.TickLabelGenerator.Generate(i * XAxis.Interval);

                if (tickLabel2 != null)
                {
                    tickLabel2.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    Canvas.SetLeft(tickLabel2, origin.Y + i * xAxisTickWidth - tickLabel2.DesiredSize.Width / 2);
                    Canvas.SetTop(tickLabel2, origin.X + XAxis.TickLabelGenerator.LabelOffset);
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

            for (int i = 1; i < XAxis.TickCount / 2; i++)
            {
                var tick = new Line();
                tick.Stroke = new SolidColorBrush(Ticks.Color);
                tick.StrokeThickness = Ticks.Thickness;
                tick.X1 = origin.Y - i * xAxisTickWidth;
                tick.Y1 = origin.X - Ticks.Width / 2;
                tick.X2 = origin.Y - i * xAxisTickWidth;
                tick.Y2 = origin.X + Ticks.Width / 2;

                graphCanvas.Children.Add(tick);

                var tick2 = new Line();
                tick2.Stroke = new SolidColorBrush(Ticks.Color);
                tick2.StrokeThickness = Ticks.Thickness;
                tick2.X1 = origin.Y + i * xAxisTickWidth;
                tick2.Y1 = origin.X - Ticks.Width / 2;
                tick2.X2 = origin.Y + i * xAxisTickWidth;
                tick2.Y2 = origin.X + Ticks.Width / 2;

                graphCanvas.Children.Add(tick2);
            }
        }

        private void DrawYAxis()
        {
            if (double.IsNaN(origin.X))
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
            yAxis.X1 = origin.X;
            yAxis.Y1 = 0;
            yAxis.X2 = origin.X;
            yAxis.Y2 = Height;

            graphCanvas.Children.Add(yAxis);
        }

        private void DrawYAxisTickLabels()
        {
            if (YAxis.TickLabelGenerator == null)
            {
                return;
            }

            for (int i = 0; i < YAxis.TickCount / 2; i++)
            {
                var tickLabel = YAxis.TickLabelGenerator.Generate(-i * YAxis.Interval);

                if (tickLabel != null)
                {
                    tickLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    Canvas.SetLeft(tickLabel, origin.X - XAxis.TickLabelGenerator.LabelOffset - tickLabel.DesiredSize.Width);
                    Canvas.SetTop(tickLabel, origin.Y + i * yAxisTickWidth - tickLabel.DesiredSize.Height / 2);

                    graphCanvas.Children.Add(tickLabel);
                }

                var tickLabel2 = YAxis.TickLabelGenerator.Generate(i * YAxis.Interval);

                if (tickLabel2 != null)
                {
                    tickLabel2.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    Canvas.SetLeft(tickLabel2, origin.X - XAxis.TickLabelGenerator.LabelOffset - tickLabel2.DesiredSize.Width);
                    Canvas.SetTop(tickLabel2, origin.Y - i * yAxisTickWidth - tickLabel2.DesiredSize.Height / 2);

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

            for (int i = 1; i < YAxis.TickCount / 2; i++)
            {
                var tick = new Line();
                tick.Stroke = new SolidColorBrush(Ticks.Color);
                tick.StrokeThickness = Ticks.Thickness;
                tick.X1 = origin.X - Ticks.Width / 2;
                tick.Y1 = origin.Y - i * yAxisTickWidth;
                tick.X2 = origin.X + Ticks.Width / 2;
                tick.Y2 = origin.Y - i * yAxisTickWidth;

                graphCanvas.Children.Add(tick);

                var tick2 = new Line();
                tick2.Stroke = new SolidColorBrush(Ticks.Color);
                tick2.StrokeThickness = Ticks.Thickness;
                tick2.X1 = origin.X - Ticks.Width / 2;
                tick2.Y1 = origin.Y + i * yAxisTickWidth;
                tick2.X2 = origin.X + Ticks.Width / 2;
                tick2.Y2 = origin.Y + i * yAxisTickWidth;

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