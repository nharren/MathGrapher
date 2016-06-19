using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MathGrapher
{
    public class DefaultTickLabelGenerator : ITickLabelGenerator
    {
        public FontFamily FontFamily { get; set; } = new FontFamily("Cambria");
        public double FontSize { get; set; } = 10.667;
        public FontStyle FontStyle { get; set; }
        public FontWeight FontWeight { get; set; }
        public Brush Foreground { get; set; } = Brushes.Black;
        public double LabelOffset { get; set; } = 7.5;

        public virtual TextBlock Generate(double value)
        {
            if (value == 0)
            {
                return null;
            }

            var tickLabel = new TextBlock();
            tickLabel.Foreground = Foreground;
            tickLabel.FontFamily = FontFamily;
            tickLabel.FontSize = FontSize;
            tickLabel.FontStyle = FontStyle;
            tickLabel.FontWeight = FontWeight;
            tickLabel.Text = value.ToString();

            return tickLabel;
        }
    }
}