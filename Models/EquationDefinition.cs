using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScaleWindows.Models
{
    public class EquationDefinition
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EquationType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FormulaLatex { get; set; } = string.Empty;
        public string Formula { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsParametricCurve { get; set; }
        public double DefaultCenterX { get; set; }
        public double DefaultCenterY { get; set; }
        public double DefaultZoom { get; set; } = 250.0;
        public int DefaultPower { get; set; } = 2;
        public int DefaultMaxIterations { get; set; } = 150;
        public double JuliaX { get; set; }
        public double JuliaY { get; set; }

        public static List<EquationDefinition> GetAll()
        {
            // Attempt to load from equations.json
            string[] possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "equations.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "equations.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "equations.json"),
                @"d:\Projects\Scale\ScaleWindows\equations.json"
            };

            foreach (string p in possiblePaths)
            {
                if (File.Exists(p))
                {
                    try
                    {
                        string json = File.ReadAllText(p);
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            Converters = { new JsonStringEnumConverter() }
                        };
                        var list = JsonSerializer.Deserialize<List<EquationDefinition>>(json, options);
                        if (list != null && list.Count > 0)
                        {
                            foreach (var item in list)
                            {
                                if (string.IsNullOrWhiteSpace(item.FormulaLatex) && !string.IsNullOrWhiteSpace(item.Formula))
                                    item.FormulaLatex = item.Formula;
                                if (string.IsNullOrWhiteSpace(item.Formula) && !string.IsNullOrWhiteSpace(item.FormulaLatex))
                                    item.Formula = item.FormulaLatex;
                            }
                            return list;
                        }
                    }
                    catch
                    {
                        // Fall through to hardcoded defaults
                    }
                }
            }

            return GetBuiltInDefaults();
        }

        public static List<EquationDefinition> GetBuiltInDefaults()
        {
            return new List<EquationDefinition>
            {
                new()
                {
                    Type = EquationType.Mandelbrot,
                    Name = "Mandelbrot Set",
                    FormulaLatex = "z_{n+1} = z_n^p + c",
                    Description = "The iconic cornerstone of chaos theory and complex dynamics.",
                    DefaultCenterX = -0.75,
                    DefaultCenterY = 0.0,
                    DefaultZoom = 250,
                    DefaultPower = 2,
                    DefaultMaxIterations = 200
                },
                new()
                {
                    Type = EquationType.Julia,
                    Name = "Julia Set",
                    FormulaLatex = "z_{n+1} = z_n^p + J",
                    Description = "Self-similar spiral filaments parameterized by a complex seed constant J.",
                    DefaultCenterX = 0.0,
                    DefaultCenterY = 0.0,
                    DefaultZoom = 300,
                    DefaultPower = 2,
                    DefaultMaxIterations = 200
                },
                new()
                {
                    Type = EquationType.BurningShip,
                    Name = "Burning Ship",
                    FormulaLatex = "z_{n+1} = (|Re(z)| + i|Im(z)|)^p + c",
                    Description = "Non-analytic variation creating gothic ship hulls, antennas, and fire flames.",
                    DefaultCenterX = -0.45,
                    DefaultCenterY = -0.55,
                    DefaultZoom = 220,
                    DefaultPower = 2,
                    DefaultMaxIterations = 200
                },
                new()
                {
                    Type = EquationType.Newton,
                    Name = "Newton-Raphson Fractal",
                    FormulaLatex = "z_{n+1} = z_n - (z_n^p - 1) / (p \\cdot z_n^{p-1})",
                    Description = "Basins of attraction from Newton's root-finding method on complex polynomials.",
                    DefaultCenterX = 0.0,
                    DefaultCenterY = 0.0,
                    DefaultZoom = 250,
                    DefaultPower = 3,
                    DefaultMaxIterations = 60
                },
                new()
                {
                    Type = EquationType.Tricorn,
                    Name = "Tricorn (Mandelbar)",
                    FormulaLatex = "z_{n+1} = (\\bar{z}_n)^p + c",
                    Description = "Antiholomorphic fractal using complex conjugation, forming three-cornered crowns.",
                    DefaultCenterX = 0.0,
                    DefaultCenterY = 0.0,
                    DefaultZoom = 240,
                    DefaultPower = 2,
                    DefaultMaxIterations = 180
                },
                new()
                {
                    Type = EquationType.SineFractal,
                    Name = "Transcendental Sine",
                    FormulaLatex = "z_{n+1} = c \\cdot \\sin(z_n)",
                    Description = "Trigonometric complex fractal producing organic ribbons and periodic cellular webs.",
                    DefaultCenterX = 0.0,
                    DefaultCenterY = 0.0,
                    DefaultZoom = 150,
                    DefaultPower = 2,
                    DefaultMaxIterations = 120
                },
                new()
                {
                    Type = EquationType.CollatzFractal,
                    Name = "Collatz Fractal (3n+1)",
                    FormulaLatex = "f(z) = \\frac{1}{4}(1 + 4z - (1 + 2z)\\cos(\\pi z))",
                    Description = "The continuous complex analytic extension of the unsolved Collatz Conjecture.",
                    DefaultCenterX = 0.0,
                    DefaultCenterY = 0.0,
                    DefaultZoom = 100,
                    DefaultPower = 1,
                    DefaultMaxIterations = 80
                },
                new()
                {
                    Type = EquationType.SumSquared,
                    Name = "(a + b)² Algebraic Fractal",
                    FormulaLatex = "z_{n+1}: x = (a+b)^2 + x_0, \\ y = 2ab + y_0",
                    Description = "Replaces complex subtraction (a² - b²) with algebraic expansion (a + b)² = a² + 2ab + b².",
                    DefaultCenterX = -0.5,
                    DefaultCenterY = 0.0,
                    DefaultZoom = 200,
                    DefaultPower = 2,
                    DefaultMaxIterations = 180
                },
                new()
                {
                    Type = EquationType.ZPlusCSquared,
                    Name = "(z + c)² Fractal",
                    FormulaLatex = "z_{n+1} = (z_n + c)^2",
                    Description = "Adds the constant before squaring instead of after: (z + c)² = z² + 2zc + c².",
                    DefaultCenterX = -0.2,
                    DefaultCenterY = 0.0,
                    DefaultZoom = 250,
                    DefaultPower = 2,
                    DefaultMaxIterations = 200
                },
                new()
                {
                    Type = EquationType.CustomEquation,
                    Name = "Custom User Equation",
                    FormulaLatex = "z_{n+1} = f(z_n, c)",
                    Description = "Type any formula like (a+b)^2, z^3+c, sin(z)+c, z^4+c to compute custom dynamics.",
                    DefaultCenterX = 0.0,
                    DefaultCenterY = 0.0,
                    DefaultZoom = 250,
                    DefaultPower = 2,
                    DefaultMaxIterations = 200
                },
                new()
                {
                    Type = EquationType.Spirograph,
                    Name = "Hypotrochoid / Spirograph",
                    FormulaLatex = "x(θ) = (R-r)\\cos θ + d\\cos((R-r)θ/r)",
                    Description = "Geometric roulette curves generated by a circle rolling inside another circle.",
                    IsParametricCurve = true,
                    DefaultCenterX = 0.0,
                    DefaultCenterY = 0.0,
                    DefaultZoom = 150
                },
                new()
                {
                    Type = EquationType.PolarRose,
                    Name = "Rhodonea (Polar Rose)",
                    FormulaLatex = "r(θ) = a \\cdot \\cos(k θ)",
                    Description = "Petaled sinusoidal curves studied by Guido Grandi in 1723.",
                    IsParametricCurve = true,
                    DefaultCenterX = 0.0,
                    DefaultCenterY = 0.0,
                    DefaultZoom = 180
                },
                new()
                {
                    Type = EquationType.Harmonograph,
                    Name = "Harmonograph (Lissajous)",
                    FormulaLatex = "x(t) = A_1\\sin(f_1 t + p_1)e^{-d_1 t}",
                    Description = "Geometric depiction of mechanical pendulum resonance and musical harmonies.",
                    IsParametricCurve = true,
                    DefaultCenterX = 0.0,
                    DefaultCenterY = 0.0,
                    DefaultZoom = 160
                }
            };
        }
    }
}
