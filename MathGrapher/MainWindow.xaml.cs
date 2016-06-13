using System;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
namespace MathGrapher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            functionBox.Text = "x => new Point(Math.Sin(x) * Math.Cos(3 * 2 * Math.PI * x), Math.Sin(x) * Math.Sin(3 * 2 * Math.PI * x))";

            CreateFunction("x => new Point(Math.Sin(x) * Math.Cos(3 * 2 * Math.PI * x), Math.Sin(x) * Math.Sin(3 * 2 * Math.PI * x))");
        }

        public async void CreateFunction(string expression)
        {
            var function = $"Func<double, Point> y = {expression};";

            var scriptOptions = ScriptOptions.Default
                .WithReferences(typeof(Point).Assembly, typeof(Func<>).Assembly)
                .WithImports("System", "System.Windows");

            var script = await CSharpScript.RunAsync<Func<double, Point>>(function, scriptOptions);
            var variable = script.GetVariable("y");

            graph.Function = (Func<double, Point>)variable.Value;

            if (graph.Function == null)
            {
                return;
            }

            graph.BeginAnimation(Graph.XProperty, null);

            var doubleAnimation = new DoubleAnimation(2 * Math.PI, new Duration(TimeSpan.FromSeconds(2 * Math.PI)));
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;

            graph.BeginAnimation(Graph.XProperty, doubleAnimation);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateFunction(functionBox.Text);
        }
    }
}