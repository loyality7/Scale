using System;
using System.Windows.Media;

namespace ScaleWindows.Core
{
    public class ColorPalette
    {
        public string Name { get; set; } = string.Empty;
        public LinearGradientBrush PreviewBrush { get; }
        private readonly uint[] _colors;
        public int Size => _colors.Length;

        public ColorPalette(string name, GradientStop[] stops, int size = 2048, double gamma = 1.0)
        {
            Name = name;
            _colors = GeneratePalette(stops, size, gamma);

            GradientStopCollection collection = new();
            foreach (var s in stops)
            {
                collection.Add(new GradientStop(s.Color, s.Offset));
            }
            PreviewBrush = new LinearGradientBrush(collection, new System.Windows.Point(0, 0), new System.Windows.Point(1, 0));
            PreviewBrush.Freeze();
        }

        private static uint[] GeneratePalette(GradientStop[] stops, int size, double gamma)
        {
            if (stops == null || stops.Length == 0)
            {
                stops = new[]
                {
                    new GradientStop(Colors.Black, 0.0),
                    new GradientStop(Colors.White, 1.0)
                };
            }

            // Ensure stops are sorted by offset
            Array.Sort(stops, (a, b) => a.Offset.CompareTo(b.Offset));

            uint[] linearColors = new uint[size];
            for (int i = 0; i < size; i++)
            {
                double t = (double)i / (size - 1);
                Color c = InterpolateStops(stops, t);
                // BGRA format for direct WriteableBitmap blitting
                linearColors[i] = (uint)((c.A << 24) | (c.R << 16) | (c.G << 8) | c.B);
            }

            if (Math.Abs(gamma - 1.0) < 0.001)
            {
                return linearColors;
            }

            // Apply gamma curve
            uint[] gammaColors = new uint[size];
            int hi = size - 1;
            for (int i = 0; i < size; i++)
            {
                double p = (double)i / hi;
                int x = (int)(hi * Math.Pow(p, gamma));
                x = Math.Clamp(x, 0, hi);
                gammaColors[i] = linearColors[x];
            }

            return gammaColors;
        }

        private static Color InterpolateStops(GradientStop[] stops, double t)
        {
            if (t <= stops[0].Offset) return stops[0].Color;
            if (t >= stops[^1].Offset) return stops[^1].Color;

            for (int i = 0; i < stops.Length - 1; i++)
            {
                if (t >= stops[i].Offset && t <= stops[i + 1].Offset)
                {
                    double range = stops[i + 1].Offset - stops[i].Offset;
                    double localT = range > 0 ? (t - stops[i].Offset) / range : 0;

                    Color c1 = stops[i].Color;
                    Color c2 = stops[i + 1].Color;

                    byte a = (byte)(c1.A + (c2.A - c1.A) * localT);
                    byte r = (byte)(c1.R + (c2.R - c1.R) * localT);
                    byte g = (byte)(c1.G + (c2.G - c1.G) * localT);
                    byte b = (byte)(c1.B + (c2.B - c1.B) * localT);

                    return Color.FromArgb(a, r, g, b);
                }
            }

            return stops[^1].Color;
        }

        public uint GetColor(int iteration, int maxIterations)
        {
            if (iteration >= maxIterations || iteration <= 0)
            {
                return 0xFF000000; // Inside set = Pure Black
            }

            int index = (int)((long)iteration * _colors.Length / maxIterations) % _colors.Length;
            return _colors[index];
        }

        public uint GetSmoothColor(double continuousIteration, int maxIterations)
        {
            if (continuousIteration >= maxIterations || continuousIteration <= 0 || double.IsNaN(continuousIteration))
            {
                return 0xFF000000;
            }

            int index = (int)((continuousIteration * 12.0) % _colors.Length);
            if (index < 0) index += _colors.Length;
            return _colors[index];
        }

        public uint GetDirectColor(double t)
        {
            t = Math.Clamp(t, 0.0, 1.0);
            int index = (int)(t * (_colors.Length - 1));
            return _colors[index];
        }
    }
}
