using System.Windows.Controls;

namespace MathGrapher
{
    public class EvenNumberTickLabelGenerator : DefaultTickLabelGenerator
    {
        public override TextBlock Generate(double value)
        {
            return value % 2 == 0 ? base.Generate(value) : null;
        }
    }
}