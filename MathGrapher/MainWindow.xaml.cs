using System;
using System.Numerics;
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
            function1.SampleCount = 10;
            //function1.IsAnimated = true;
            function1.ConnectSamples = false;
            function1.Function = x =>
            {
                var z = Complex.Pow(Complex.ImaginaryOne, x);
                return new Point(x, z.Real);
            };

            graph.FunctionDefinitions.Add(function1);

            var function2 = new FunctionDefinition();
            function2.Brush = Brushes.Blue;
            function2.Thickness = 8;
            function2.SampleCount = 10;
            function2.ConnectSamples = false;
            //function2.IsAnimated = true;
            function2.Function = x =>
            {
                var z = Complex.Pow(Complex.ImaginaryOne, x);
                return new Point(x, z.Imaginary);
            };

            graph.FunctionDefinitions.Add(function2);

            //graph.Animation = new GraphAnimation(-.00001, 1000, 5, 24);

            graph.LegendDefinitions.Add(new LegendDefinition { Brush = Brushes.Red, Description = "Real" });
            graph.LegendDefinitions.Add(new LegendDefinition { Brush = Brushes.Blue, Description = "Imaginary" });

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            graph.Start();
        }
    }
}