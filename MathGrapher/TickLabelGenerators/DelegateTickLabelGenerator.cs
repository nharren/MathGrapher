using System;
using System.Windows.Controls;

namespace MathGrapher
{
    public class DelegateTickLabelGenerator : ITickLabelGenerator
    {
        public Func<double, bool> CanGenerateFunction { get; set; }
        public Func<double, TextBlock> GenerateFunction { get; set; }
        public double LabelOffset { get; set; }

        public TextBlock Generate(double value)
        {
            return CanGenerateFunction(value) ? GenerateFunction(value) : null;
        }
    }
}