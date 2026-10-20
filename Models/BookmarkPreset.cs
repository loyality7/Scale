using System.Collections.Generic;

namespace ScaleWindows.Models
{
    public class BookmarkPreset
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public EquationType Equation { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double Zoom { get; set; }
        public int Power { get; set; } = 2;
        public int MaxIterations { get; set; } = 300;
        public double JuliaX { get; set; }
        public double JuliaY { get; set; }
        public string SuggestedPalette { get; set; } = "Electric Neon";

        public static List<BookmarkPreset> GetDefaults()
        {
            return new List<BookmarkPreset>
            {
                // Michael Fogleman's Original Scale Reference Landmarks
                new()
                {
                    Title = "Quad-Spiral Julia (Scale Reference)",
                    Category = "Reference",
                    Equation = EquationType.Julia,
                    CenterX = 0.0,
                    CenterY = 0.0,
                    Zoom = 256.0,
                    Power = 4,
                    MaxIterations = 32,
                    JuliaX = 0.5230957269668579,
                    JuliaY = -0.6428906321525574,
                    SuggestedPalette = "Electric Cyan & Blue"
                },
                new()
                {
                    Title = "Cubic Star Julia (Scale Reference)",
                    Category = "Reference",
                    Equation = EquationType.Julia,
                    CenterX = 0.0,
                    CenterY = 0.28,
                    Zoom = 2048.0,
                    Power = 3,
                    MaxIterations = 256,
                    JuliaX = 0.0,
                    JuliaY = -1.09,
                    SuggestedPalette = "Cosmic Violet & Purple"
                },
                new()
                {
                    Title = "Deep Spiral Filigree (Scale Reference)",
                    Category = "Reference",
                    Equation = EquationType.Julia,
                    CenterX = -0.0729261264,
                    CenterY = -0.7413022518,
                    Zoom = 524288.0,
                    Power = 2,
                    MaxIterations = 512,
                    JuliaX = -0.07421875,
                    JuliaY = -0.66015625,
                    SuggestedPalette = "Warm Amber & Honey"
                },
                new()
                {
                    Title = "Cubic Lattice Julia (Scale Reference)",
                    Category = "Reference",
                    Equation = EquationType.Julia,
                    CenterX = -0.4936122894,
                    CenterY = 0.0812568665,
                    Zoom = 2048.0,
                    Power = 3,
                    MaxIterations = 1024,
                    JuliaX = -0.02734375,
                    JuliaY = -0.76953125,
                    SuggestedPalette = "Emerald Matrix"
                },
                new()
                {
                    Title = "Twin Lightning Julia (Scale Reference)",
                    Category = "Reference",
                    Equation = EquationType.Julia,
                    CenterX = 0.0201969147,
                    CenterY = 0.578207016,
                    Zoom = 2048.0,
                    Power = 3,
                    MaxIterations = 512,
                    JuliaX = 0.0412500016,
                    JuliaY = 0.774345696,
                    SuggestedPalette = "Cyberpunk Neon"
                },
                new()
                {
                    Title = "Pentagonal Star Julia (Scale Reference)",
                    Category = "Reference",
                    Equation = EquationType.Julia,
                    CenterX = 0.0,
                    CenterY = 0.0,
                    Zoom = 256.0,
                    Power = 5,
                    MaxIterations = 256,
                    JuliaX = -0.145957127,
                    JuliaY = -0.678979099,
                    SuggestedPalette = "Inferno Fire"
                },
                new()
                {
                    Title = "Scepter Valley Satellite (Scale Reference)",
                    Category = "Reference",
                    Equation = EquationType.Mandelbrot,
                    CenterX = -1.2533140257,
                    CenterY = 0.0257614627,
                    Zoom = 2097152.0,
                    Power = 2,
                    MaxIterations = 512,
                    SuggestedPalette = "Obsidian Gold"
                },
                new()
                {
                    Title = "Elephant Valley Deep Spiral (Scale Reference)",
                    Category = "Reference",
                    Equation = EquationType.Mandelbrot,
                    CenterX = -0.3283522911,
                    CenterY = 0.6184638645,
                    Zoom = 2097152.0,
                    Power = 2,
                    MaxIterations = 2048,
                    SuggestedPalette = "Deep Ocean Abyss"
                },
                new()
                {
                    Title = "Cubic Needle (Scale Reference)",
                    Category = "Reference",
                    Equation = EquationType.Mandelbrot,
                    CenterX = 0.3952303206,
                    CenterY = 0.0018339272,
                    Zoom = 67108864.0,
                    Power = 3,
                    MaxIterations = 512,
                    SuggestedPalette = "Sunset Coral & Gold"
                },

                // Classical Chaos Landmarks
                new()
                {
                    Title = "Seahorse Valley Spiral",
                    Category = "Mandelbrot",
                    Equation = EquationType.Mandelbrot,
                    CenterX = -0.743643887,
                    CenterY = 0.131825904,
                    Zoom = 150000.0,
                    Power = 2,
                    MaxIterations = 450,
                    SuggestedPalette = "Warm Amber & Honey"
                },
                new()
                {
                    Title = "Elephant Valley",
                    Category = "Mandelbrot",
                    Equation = EquationType.Mandelbrot,
                    CenterX = 0.281,
                    CenterY = 0.008,
                    Zoom = 4500.0,
                    Power = 2,
                    MaxIterations = 350,
                    SuggestedPalette = "Obsidian Gold"
                },
                new()
                {
                    Title = "Golden Ratio Spiral",
                    Category = "Julia",
                    Equation = EquationType.Julia,
                    CenterX = 0.0,
                    CenterY = 0.0,
                    Zoom = 340.0,
                    Power = 2,
                    MaxIterations = 300,
                    JuliaX = -0.7,
                    JuliaY = 0.27015,
                    SuggestedPalette = "Warm Amber"
                },
                new()
                {
                    Title = "Dendrite Lightning Tree",
                    Category = "Julia",
                    Equation = EquationType.Julia,
                    CenterX = 0.0,
                    CenterY = 0.0,
                    Zoom = 320.0,
                    Power = 2,
                    MaxIterations = 350,
                    JuliaX = 0.0,
                    JuliaY = 1.0,
                    SuggestedPalette = "Forest & Jade"
                },
                new()
                {
                    Title = "Dragon Flame Julia",
                    Category = "Julia",
                    Equation = EquationType.Julia,
                    CenterX = 0.0,
                    CenterY = 0.0,
                    Zoom = 380.0,
                    Power = 2,
                    MaxIterations = 400,
                    JuliaX = -0.8,
                    JuliaY = 0.156,
                    SuggestedPalette = "Solar Flare"
                },
                new()
                {
                    Title = "Burning Ship Mast",
                    Category = "Burning Ship",
                    Equation = EquationType.BurningShip,
                    CenterX = -1.758,
                    CenterY = -0.032,
                    Zoom = 8000.0,
                    Power = 2,
                    MaxIterations = 400,
                    SuggestedPalette = "Solar Flare"
                },
                new()
                {
                    Title = "Newton 5-Leaf Basins",
                    Category = "Newton",
                    Equation = EquationType.Newton,
                    CenterX = 0.0,
                    CenterY = 0.0,
                    Zoom = 300.0,
                    Power = 5,
                    MaxIterations = 80,
                    SuggestedPalette = "Obsidian Gold"
                },
                new()
                {
                    Title = "Tricorn 3-Way Crown",
                    Category = "Tricorn",
                    Equation = EquationType.Tricorn,
                    CenterX = 0.0,
                    CenterY = 0.0,
                    Zoom = 250.0,
                    Power = 2,
                    MaxIterations = 250,
                    SuggestedPalette = "Terracotta Sunset"
                },
                new()
                {
                    Title = "Sine Wave Lattice",
                    Category = "Transcendental",
                    Equation = EquationType.SineFractal,
                    CenterX = 0.0,
                    CenterY = 0.0,
                    Zoom = 140.0,
                    Power = 2,
                    MaxIterations = 150,
                    SuggestedPalette = "Bronze Horizon"
                },
                new()
                {
                    Title = "(a + b)² Algebraic Heart",
                    Category = "Algebraic",
                    Equation = EquationType.SumSquared,
                    CenterX = -0.5,
                    CenterY = 0.0,
                    Zoom = 260.0,
                    MaxIterations = 250,
                    SuggestedPalette = "Solar Flare"
                },
                new()
                {
                    Title = "(z + c)² Pre-Sum Fractal",
                    Category = "Algebraic",
                    Equation = EquationType.ZPlusCSquared,
                    CenterX = -0.15,
                    CenterY = 0.0,
                    Zoom = 280.0,
                    MaxIterations = 220,
                    SuggestedPalette = "Warm Amber"
                },
                new()
                {
                    Title = "7-Petal Rhodonea Rose",
                    Category = "Curves",
                    Equation = EquationType.PolarRose,
                    CenterX = 0.0,
                    CenterY = 0.0,
                    Zoom = 180.0,
                    SuggestedPalette = "Warm Amber"
                },
                new()
                {
                    Title = "Harmonic Pendulum Resonance",
                    Category = "Curves",
                    Equation = EquationType.Harmonograph,
                    CenterX = 0.0,
                    CenterY = 0.0,
                    Zoom = 160.0,
                    SuggestedPalette = "Obsidian Gold"
                }
            };
        }
    }
}
