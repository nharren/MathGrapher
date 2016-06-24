using System;
using System.Collections.Generic;

namespace MathGrapher
{
    public class GraphAnimation
    {
        public GraphAnimation()
        {
        }

        public GraphAnimation(double from, double to, double seconds, int fps)
        {
            var frameCount = seconds * fps;
            var interval = (to - from) / frameCount;

            for (int i = 0; i < frameCount; i++)
            {
                Values.Add(from + i * interval);
            }

            Delay = TimeSpan.FromSeconds(1.0 / fps);
        }

        public bool Repeat { get; set; } = true;
        public TimeSpan Delay { get; set; }
        public List<double> Values { get; } = new List<double>();
    }
}