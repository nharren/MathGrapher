using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MathGrapher
{
    public class ImaginaryWholeNumberTickLabelGenerator : WholeNumberTickLabelGenerator
    {
        public override TextBlock Generate(double value)
        {
            var textBlock = base.Generate(value);

            if (textBlock != null)
            {
                if (textBlock.Text == "1")
                {
                    textBlock.Text = string.Empty;
                }

                if (textBlock.Text == "-1")
                {
                    textBlock.Text = "-";
                }

                textBlock.Inlines.Add(new Run("i") { FontStyle = FontStyles.Italic});
            }
            
            return textBlock;
        }
    }
}