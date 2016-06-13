using System.Windows;
using System.Windows.Media;

namespace MathGrapher
{
    public class TickLabelsDefinition
    {
        public FontFamily FontFamily { get; set; }
        public double FontSize { get; set; }
        public FontStyle FontStyle { get; set; }
        public FontWeight FontWeight { get; set; }
        public Brush Foreground { get; set; }
        public int Frequency { get; set; }
        public double Offset { get; set; }
    }
}