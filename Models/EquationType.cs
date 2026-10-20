namespace ScaleWindows.Models
{
    public enum EquationType
    {
        // Core Classical Fractals
        Mandelbrot,
        Julia,
        BurningShip,
        Newton,
        Tricorn,

        // Advanced Transcendental & Custom Equations
        SineFractal,
        CollatzFractal,
        SumSquared,       // (a + b)^2 + c
        ZPlusCSquared,    // (z + c)^2
        CustomEquation,   // User-defined algebraic / mathematical formula

        // Parametric Math Art Curves
        Spirograph,
        PolarRose,
        Harmonograph
    }
}
