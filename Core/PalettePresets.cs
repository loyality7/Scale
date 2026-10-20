using System.Collections.Generic;
using System.Windows.Media;

namespace ScaleWindows.Core
{
    public static class PalettePresets
    {
        public static List<ColorPalette> GetPresets(double gamma = 1.0)
        {
            return new List<ColorPalette>
            {
                // 1. Electric Cyan & Blue (Default)
                new("Electric Cyan & Blue", new[]
                {
                    new GradientStop(Color.FromRgb(2, 6, 20), 0.0),
                    new GradientStop(Color.FromRgb(0, 70, 180), 0.3),
                    new GradientStop(Color.FromRgb(0, 190, 255), 0.65),
                    new GradientStop(Color.FromRgb(150, 235, 255), 0.85),
                    new GradientStop(Color.FromRgb(255, 255, 255), 1.0)
                }, 2048, gamma),

                // 2. Full Rainbow Spectrum
                new("Full Rainbow Spectrum", new[]
                {
                    new GradientStop(Color.FromRgb(255, 0, 0), 0.0),      // Red
                    new GradientStop(Color.FromRgb(255, 127, 0), 0.14),   // Orange
                    new GradientStop(Color.FromRgb(255, 255, 0), 0.28),   // Yellow
                    new GradientStop(Color.FromRgb(0, 230, 64), 0.42),    // Green
                    new GradientStop(Color.FromRgb(0, 210, 255), 0.57),   // Cyan
                    new GradientStop(Color.FromRgb(0, 80, 255), 0.71),    // Blue
                    new GradientStop(Color.FromRgb(170, 0, 255), 0.85),   // Violet
                    new GradientStop(Color.FromRgb(255, 0, 128), 1.0)     // Magenta
                }, 2048, gamma),

                // 3. Cosmic Violet & Purple
                new("Cosmic Violet & Purple", new[]
                {
                    new GradientStop(Color.FromRgb(10, 2, 25), 0.0),
                    new GradientStop(Color.FromRgb(90, 15, 160), 0.35),
                    new GradientStop(Color.FromRgb(175, 45, 240), 0.7),
                    new GradientStop(Color.FromRgb(230, 160, 255), 0.9),
                    new GradientStop(Color.FromRgb(255, 255, 255), 1.0)
                }, 2048, gamma),

                // 4. Cyberpunk Neon
                new("Cyberpunk Neon", new[]
                {
                    new GradientStop(Color.FromRgb(15, 0, 30), 0.0),
                    new GradientStop(Color.FromRgb(230, 0, 120), 0.35),   // Hot Pink
                    new GradientStop(Color.FromRgb(0, 235, 220), 0.7),    // Neon Cyan
                    new GradientStop(Color.FromRgb(255, 230, 80), 0.9),   // Bright Yellow
                    new GradientStop(Color.FromRgb(255, 255, 255), 1.0)
                }, 2048, gamma),

                // 5. Inferno Fire & Ember
                new("Inferno Fire", new[]
                {
                    new GradientStop(Color.FromRgb(8, 0, 0), 0.0),
                    new GradientStop(Color.FromRgb(160, 20, 0), 0.28),
                    new GradientStop(Color.FromRgb(255, 110, 0), 0.6),
                    new GradientStop(Color.FromRgb(255, 225, 60), 0.85),
                    new GradientStop(Color.FromRgb(255, 255, 255), 1.0)
                }, 2048, gamma),

                // 6. Emerald & Jade Matrix
                new("Emerald Matrix", new[]
                {
                    new GradientStop(Color.FromRgb(2, 10, 4), 0.0),
                    new GradientStop(Color.FromRgb(0, 85, 35), 0.3),
                    new GradientStop(Color.FromRgb(20, 215, 90), 0.65),
                    new GradientStop(Color.FromRgb(160, 255, 185), 0.88),
                    new GradientStop(Color.FromRgb(255, 255, 255), 1.0)
                }, 2048, gamma),

                // 7. Deep Ocean Abyss
                new("Deep Ocean Abyss", new[]
                {
                    new GradientStop(Color.FromRgb(1, 4, 18), 0.0),
                    new GradientStop(Color.FromRgb(0, 40, 100), 0.35),
                    new GradientStop(Color.FromRgb(0, 140, 175), 0.7),
                    new GradientStop(Color.FromRgb(120, 230, 240), 0.9),
                    new GradientStop(Color.FromRgb(255, 255, 255), 1.0)
                }, 2048, gamma),

                // 8. Sunset Coral & Gold
                new("Sunset Coral & Gold", new[]
                {
                    new GradientStop(Color.FromRgb(25, 8, 35), 0.0),
                    new GradientStop(Color.FromRgb(140, 25, 80), 0.3),
                    new GradientStop(Color.FromRgb(235, 75, 45), 0.6),
                    new GradientStop(Color.FromRgb(255, 185, 60), 0.85),
                    new GradientStop(Color.FromRgb(255, 250, 210), 1.0)
                }, 2048, gamma),

                // 9. Warm Amber & Honey
                new("Warm Amber & Honey", new[]
                {
                    new GradientStop(Color.FromRgb(12, 6, 2), 0.0),
                    new GradientStop(Color.FromRgb(120, 48, 12), 0.3),
                    new GradientStop(Color.FromRgb(240, 138, 40), 0.65),
                    new GradientStop(Color.FromRgb(255, 210, 120), 0.88),
                    new GradientStop(Color.FromRgb(255, 252, 240), 1.0)
                }, 2048, gamma),

                // 10. Obsidian & Metallic Gold
                new("Obsidian Gold", new[]
                {
                    new GradientStop(Color.FromRgb(4, 4, 4), 0.0),
                    new GradientStop(Color.FromRgb(55, 42, 18), 0.3),
                    new GradientStop(Color.FromRgb(190, 145, 45), 0.65),
                    new GradientStop(Color.FromRgb(245, 215, 115), 0.88),
                    new GradientStop(Color.FromRgb(255, 255, 240), 1.0)
                }, 2048, gamma),

                // 11. Acid Lime & Teal
                new("Acid Lime & Teal", new[]
                {
                    new GradientStop(Color.FromRgb(0, 15, 20), 0.0),
                    new GradientStop(Color.FromRgb(0, 95, 110), 0.3),
                    new GradientStop(Color.FromRgb(50, 200, 100), 0.65),
                    new GradientStop(Color.FromRgb(200, 255, 50), 0.88),
                    new GradientStop(Color.FromRgb(255, 255, 255), 1.0)
                }, 2048, gamma),

                // 12. Ice & Frost
                new("Ice & Frost", new[]
                {
                    new GradientStop(Color.FromRgb(8, 15, 28), 0.0),
                    new GradientStop(Color.FromRgb(35, 75, 120), 0.35),
                    new GradientStop(Color.FromRgb(95, 175, 215), 0.7),
                    new GradientStop(Color.FromRgb(200, 240, 255), 0.9),
                    new GradientStop(Color.FromRgb(255, 255, 255), 1.0)
                }, 2048, gamma),

                // 13. Pastel Dreams
                new("Pastel Dreams", new[]
                {
                    new GradientStop(Color.FromRgb(40, 25, 45), 0.0),
                    new GradientStop(Color.FromRgb(150, 130, 190), 0.3),
                    new GradientStop(Color.FromRgb(140, 195, 220), 0.6),
                    new GradientStop(Color.FromRgb(240, 185, 180), 0.85),
                    new GradientStop(Color.FromRgb(255, 245, 230), 1.0)
                }, 2048, gamma),

                // 14. Monochrome Noir
                new("Monochrome Noir", new[]
                {
                    new GradientStop(Color.FromRgb(0, 0, 0), 0.0),
                    new GradientStop(Color.FromRgb(45, 45, 45), 0.3),
                    new GradientStop(Color.FromRgb(125, 125, 125), 0.65),
                    new GradientStop(Color.FromRgb(215, 215, 215), 0.88),
                    new GradientStop(Color.FromRgb(255, 255, 255), 1.0)
                }, 2048, gamma)
            };
        }
    }
}
