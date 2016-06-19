using System.Windows.Controls;

namespace MathGrapher
{
    public interface ITickLabelGenerator
    {
        double LabelOffset { get; set; }

        TextBlock Generate(double value);
    }
}