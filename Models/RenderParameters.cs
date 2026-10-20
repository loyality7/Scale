using System;

namespace ScaleWindows.Models
{
    public class RenderParameters : ICloneable
    {
        public EquationType Equation { get; set; } = EquationType.Mandelbrot;
        public double CenterX { get; set; } = -0.75;
        public double CenterY { get; set; } = 0.0;
        public double Zoom { get; set; } = 250.0;
        public int Power { get; set; } = 2;
        public int MaxIterations { get; set; } = 200;
        public double JuliaX { get; set; } = -0.7;
        public double JuliaY { get; set; } = 0.27015; // Classic golden spiral Julia
        public double Gamma { get; set; } = 1.0;
        public int Antialiasing { get; set; } = 1; // 1 = 1x, 2 = 2x supersampling
        public bool SmoothColoring { get; set; } = true;
        public string CustomFormula { get; set; } = "(a + b)^2 + c";

        // Parametric curve parameters
        public double CurveParamR { get; set; } = 8.0;
        public double CurveParamLittleR { get; set; } = 1.0;
        public double CurveParamD { get; set; } = 4.0;
        public double CurveRoseK { get; set; } = 7.0;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
