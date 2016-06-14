using System;
using System.Windows;

namespace MathGrapher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            graph.Function = x => new Point(Math.Cos(x), Math.Sin(x));
        }
    }
}