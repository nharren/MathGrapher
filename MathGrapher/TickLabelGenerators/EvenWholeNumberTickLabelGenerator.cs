using System.Windows.Controls;

namespace MathGrapher
{
    internal class EvenWholeNumberTickLabelGenerator : WholeNumberTickLabelGenerator
    {
        public override TextBlock Generate(double value)
        {
            return value % 2 == 0 ? base.Generate(value) : null;
        }
    }
}