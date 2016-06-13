using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace MathGrapher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            graph.Function = x => new Point(Math.Sin(x) * Math.Cos(3 * 2 * Math.PI * x), Math.Sin(x) * Math.Sin(3 * 2 * Math.PI * x));

            var doubleAnimation = new DoubleAnimation(Math.PI, new Duration(TimeSpan.FromSeconds(Math.PI)));
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;

            graph.BeginAnimation(Graph.XProperty, doubleAnimation);
        }
    }
}