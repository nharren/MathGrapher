using System;
using System.Windows;
using System.Windows.Media;

namespace MathGrapher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var function1 = new FunctionDefinition();
            function1.Brush = Brushes.Red;
            function1.Thickness = 8;
            function1.SampleCount = 1000;
            function1.IsAnimated = true;
            function1.Function = x => new Point(x, Math.Cos(x));
            graph.FunctionDefinitions.Add(function1);

            var function2 = new FunctionDefinition();
            function2.Brush = Brushes.Blue;
            function2.Thickness = 8;
            function2.SampleCount = 1000;
            function2.IsAnimated = true;
            function2.Function = x => new Point(x, Math.Sin(x));
            graph.FunctionDefinitions.Add(function2);

            graph.Animation = new GraphAnimation(-10, 10, 5, 24);

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            graph.Start();
        }
    }
}