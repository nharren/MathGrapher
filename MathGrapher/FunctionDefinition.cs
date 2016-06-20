using System;
using System.Windows;
using System.Windows.Media;

namespace MathGrapher
{
    public class FunctionDefinition
    {
        public Brush Brush { get; set; }
        public Vector Domain { get; set; }
        public Func<double, Point> Function { get; set; }
        public bool IsAnimated { get; set; }
        public int SampleCount { get; set; }
        public double Thickness { get; set; }
    }
}