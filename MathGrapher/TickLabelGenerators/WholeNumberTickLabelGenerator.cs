using System.Windows.Controls;

namespace MathGrapher
{
    public class WholeNumberTickLabelGenerator : DefaultTickLabelGenerator
    {
        public override TextBlock Generate(double value)
        {
            return value % 1 == 0 ? base.Generate(value) : null;
        }
    }
}