using System;
using System.Windows;

namespace MathGrapher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            graph.Function = x => new Point(Math.Cos(Math.PI * x / 2), Math.Sin(Math.PI * x / 2));
        }
    }
}