using System.Windows.Media;

namespace MathGrapher
{
    public class AxisDefinition
    {
        public Color Color { get; set; } = Colors.Black;
        public double Min { get; set; } = -10;
        public double Max { get; set; } = 10;
        public double Interval { get; set; } = 1;
        public double Thickness { get; set; } = 1;
        public ITickLabelGenerator TickLabelGenerator { get; set; } = new DefaultTickLabelGenerator();

        internal double TickCount
        {
            get
            {
                return (Max - Min) / Interval;
            }
        }
    }
}