using System;
using System.Threading;
using System.Threading.Tasks;
using ScaleWindows.Core;
using ScaleWindows.Models;

namespace ScaleWindows.Rendering
{
    public class FractalRenderer
    {
        public static bool Render(
            RenderParameters parameters,
            ColorPalette palette,
            int width,
            int height,
            uint[] pixelBuffer,
            CancellationToken cancellationToken,
            IProgress<double>? progress = null)
        {
            if (pixelBuffer.Length < width * height) return false;

            double zoom = parameters.Zoom;
            double centerX = parameters.CenterX;
            double centerY = parameters.CenterY;
            int power = Math.Max(2, parameters.Power);
            int maxIter = parameters.MaxIterations;
            int aa = Math.Max(1, parameters.Antialiasing);
            bool smooth = parameters.SmoothColoring;

            double jx = parameters.JuliaX;
            double jy = parameters.JuliaY;

            double invZoom = 1.0 / zoom;
            double halfW = width * 0.5;
            double halfH = height * 0.5;

            CustomExpressionParser parser = new(parameters.CustomFormula);

            // Parallel loop over scanlines across all CPU cores
            ParallelOptions options = new() { CancellationToken = cancellationToken };
            int completedRows = 0;
            int reportInterval = Math.Max(1, height / 100);

            try
            {
                Parallel.For(0, height, options, y =>
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    int rowOffset = y * width;
                    double wy = centerY + (halfH - y) * invZoom;

                    for (int x = 0; x < width; x++)
                    {
                        double wx = centerX + (x - halfW) * invZoom;

                        if (aa == 1)
                        {
                            pixelBuffer[rowOffset + x] = ComputePixel(
                                parameters.Equation, wx, wy, power, maxIter, jx, jy, palette, smooth, parser);
                        }
                        else
                        {
                            // 2x2 Supersampling Antialiasing
                            double subStep = 0.5 * invZoom;
                            uint c0 = ComputePixel(parameters.Equation, wx - subStep * 0.5, wy - subStep * 0.5, power, maxIter, jx, jy, palette, smooth, parser);
                            uint c1 = ComputePixel(parameters.Equation, wx + subStep * 0.5, wy - subStep * 0.5, power, maxIter, jx, jy, palette, smooth, parser);
                            uint c2 = ComputePixel(parameters.Equation, wx - subStep * 0.5, wy + subStep * 0.5, power, maxIter, jx, jy, palette, smooth, parser);
                            uint c3 = ComputePixel(parameters.Equation, wx + subStep * 0.5, wy + subStep * 0.5, power, maxIter, jx, jy, palette, smooth, parser);

                            pixelBuffer[rowOffset + x] = Blend4Colors(c0, c1, c2, c3);
                        }
                    }

                    if (progress != null)
                    {
                        int done = Interlocked.Increment(ref completedRows);
                        if (done % reportInterval == 0 || done == height)
                        {
                            // Scale rendering progress between 0% and 95%
                            progress.Report((double)done / height * 95.0);
                        }
                    }
                });
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        private static uint ComputePixel(
            EquationType equation,
            double x0,
            double y0,
            int power,
            int maxIter,
            double jx,
            double jy,
            ColorPalette palette,
            bool smooth,
            CustomExpressionParser parser)
        {
            return equation switch
            {
                EquationType.Mandelbrot => ComputeMandelbrot(x0, y0, power, maxIter, palette, smooth),
                EquationType.Julia => ComputeJulia(x0, y0, jx, jy, power, maxIter, palette, smooth),
                EquationType.BurningShip => ComputeBurningShip(x0, y0, power, maxIter, palette, smooth),
                EquationType.Newton => ComputeNewton(x0, y0, power, maxIter, palette),
                EquationType.Tricorn => ComputeTricorn(x0, y0, power, maxIter, palette, smooth),
                EquationType.SineFractal => ComputeSineFractal(x0, y0, maxIter, palette),
                EquationType.CollatzFractal => ComputeCollatz(x0, y0, maxIter, palette),
                EquationType.SumSquared => ComputeSumSquared(x0, y0, maxIter, palette, smooth),
                EquationType.ZPlusCSquared => ComputeZPlusCSquared(x0, y0, maxIter, palette, smooth),
                EquationType.CustomEquation => ComputeCustom(x0, y0, parser, maxIter, palette, smooth),
                _ => ComputeMandelbrot(x0, y0, power, maxIter, palette, smooth)
            };
        }

        private static uint ComputeCustom(double x0, double y0, CustomExpressionParser parser, int maxIter, ColorPalette palette, bool smooth)
        {
            ComplexNumber c = new(x0, y0);

            // If f(0, c) == 0, then z=0 is a trivial dead trap where z would stay 0 forever.
            // In that case, z must start as the pixel coordinate (x0, y0).
            ComplexNumber testZero = parser.EvaluateStep(new ComplexNumber(0, 0), c, x0, y0);
            ComplexNumber z = testZero.MagnitudeSquared < 1e-14 ? c : testZero;

            int iter = 0;
            double escapeRadiusSq = 100.0;
            ComplexNumber prevZ = z;

            while (z.MagnitudeSquared <= escapeRadiusSq && iter < maxIter)
            {
                prevZ = z;
                z = parser.EvaluateStep(z, c, x0, y0);

                // Convergence check (attractor dynamics): if delta is tiny, it's trapped in a limit cycle or root
                ComplexNumber diff = z - prevZ;
                if (iter > 1 && diff.MagnitudeSquared < 1e-8)
                {
                    return palette.GetColor(iter, maxIter);
                }

                // If NaN or Infinity occurred (e.g. division by 0 singularity)
                if (double.IsNaN(z.Real) || double.IsInfinity(z.Real))
                {
                    return palette.GetColor(iter, maxIter);
                }

                iter++;
            }

            if (iter >= maxIter) return 0xFF000000;

            if (smooth && z.MagnitudeSquared > 1.0)
            {
                double mag = Math.Sqrt(z.MagnitudeSquared);
                double continuous = iter + 1 - Math.Log(Math.Max(1.0, Math.Log(mag)));
                return palette.GetSmoothColor(continuous, maxIter);
            }

            return palette.GetColor(iter, maxIter);
        }

        private static uint ComputeMandelbrot(double x0, double y0, int power, int maxIter, ColorPalette palette, bool smooth)
        {
            // Cardioid and period-2 bulb quick check for power 2
            if (power == 2)
            {
                double q = (x0 - 0.25) * (x0 - 0.25) + y0 * y0;
                if (q * (q + (x0 - 0.25)) < 0.25 * y0 * y0) return 0xFF000000;
                if ((x0 + 1) * (x0 + 1) + y0 * y0 < 0.0625) return 0xFF000000;
            }

            double x = 0;
            double y = 0;
            int iter = 0;
            int p = power - 1;

            while (x * x + y * y <= 4.0 && iter < maxIter)
            {
                double a = x;
                double b = y;
                for (int i = 0; i < p; i++)
                {
                    double temp = a * x - b * y;
                    y = b * x + a * y;
                    x = temp;
                }
                x += x0;
                y += y0;
                iter++;
            }

            if (iter >= maxIter) return 0xFF000000;

            if (smooth)
            {
                double mag = Math.Sqrt(x * x + y * y);
                double continuous = iter + 1 - Math.Log(Math.Max(1.0, Math.Log(mag))) / Math.Log(power);
                return palette.GetSmoothColor(continuous, maxIter);
            }
            return palette.GetColor(iter, maxIter);
        }

        private static uint ComputeJulia(double x0, double y0, double jx, double jy, int power, int maxIter, ColorPalette palette, bool smooth)
        {
            double x = x0;
            double y = y0;
            int iter = 0;
            int p = power - 1;

            while (x * x + y * y <= 4.0 && iter < maxIter)
            {
                double a = x;
                double b = y;
                for (int i = 0; i < p; i++)
                {
                    double temp = a * x - b * y;
                    y = b * x + a * y;
                    x = temp;
                }
                x += jx;
                y += jy;
                iter++;
            }

            if (iter >= maxIter) return 0xFF000000;

            if (smooth)
            {
                double mag = Math.Sqrt(x * x + y * y);
                double continuous = iter + 1 - Math.Log(Math.Max(1.0, Math.Log(mag))) / Math.Log(power);
                return palette.GetSmoothColor(continuous, maxIter);
            }
            return palette.GetColor(iter, maxIter);
        }

        private static uint ComputeBurningShip(double x0, double y0, int power, int maxIter, ColorPalette palette, bool smooth)
        {
            double x = 0;
            double y = 0;
            int iter = 0;
            int p = power - 1;

            while (x * x + y * y <= 4.0 && iter < maxIter)
            {
                double a = Math.Abs(x);
                double b = Math.Abs(y);
                for (int i = 0; i < p; i++)
                {
                    double temp = a * Math.Abs(x) - b * Math.Abs(y);
                    b = b * Math.Abs(x) + a * Math.Abs(y);
                    a = temp;
                }
                x = a + x0;
                y = b + y0;
                iter++;
            }

            if (iter >= maxIter) return 0xFF000000;
            return palette.GetColor(iter, maxIter);
        }

        private static uint ComputeTricorn(double x0, double y0, int power, int maxIter, ColorPalette palette, bool smooth)
        {
            double x = 0;
            double y = 0;
            int iter = 0;
            int p = power - 1;

            while (x * x + y * y <= 4.0 && iter < maxIter)
            {
                double a = x;
                double b = -y; // Complex conjugate
                for (int i = 0; i < p; i++)
                {
                    double temp = a * x - b * -y;
                    y = -(b * x + a * -y);
                    x = temp;
                }
                x += x0;
                y += y0;
                iter++;
            }

            if (iter >= maxIter) return 0xFF000000;
            return palette.GetColor(iter, maxIter);
        }

        private static uint ComputeNewton(double x0, double y0, int power, int maxIter, ColorPalette palette)
        {
            ComplexNumber z = new(x0, y0);
            int p = Math.Max(3, power);

            for (int iter = 0; iter < maxIter; iter++)
            {
                ComplexNumber zP = z.Pow(p);
                ComplexNumber fz = zP - new ComplexNumber(1, 0);

                if (fz.MagnitudeSquared < 1e-6)
                {
                    // Map root angle to color
                    double angle = Math.Atan2(z.Imaginary, z.Real);
                    if (angle < 0) angle += 2 * Math.PI;
                    double t = (angle / (2 * Math.PI) + (double)iter / maxIter * 0.3) % 1.0;
                    return palette.GetDirectColor(t);
                }

                ComplexNumber dfz = z.Pow(p - 1) * p;
                if (dfz.MagnitudeSquared < 1e-10) break;

                z -= fz / dfz;
            }

            return 0xFF000000;
        }

        private static uint ComputeSineFractal(double x0, double y0, int maxIter, ColorPalette palette)
        {
            ComplexNumber z = new(x0, y0);
            ComplexNumber c = new(1.0, 0.2);

            int iter = 0;
            while (z.MagnitudeSquared <= 50.0 && iter < maxIter)
            {
                z = z.Sin() * c;
                iter++;
            }

            if (iter >= maxIter) return 0xFF000000;
            return palette.GetColor(iter, maxIter);
        }

        private static uint ComputeCollatz(double x0, double y0, int maxIter, ColorPalette palette)
        {
            ComplexNumber z = new(x0, y0);
            ComplexNumber one = new(1, 0);
            ComplexNumber two = new(2, 0);
            ComplexNumber four = new(4, 0);
            double pi = Math.PI;

            int iter = 0;
            while (z.MagnitudeSquared <= 100.0 && iter < maxIter)
            {
                // f(z) = 0.25 * (1 + 4z - (1 + 2z) * cos(pi * z))
                ComplexNumber piZ = z * pi;
                ComplexNumber cosTerm = piZ.Cos();
                ComplexNumber term = one + z * four - (one + z * two) * cosTerm;
                z = term * 0.25;
                iter++;
            }

            if (iter >= maxIter) return 0xFF000000;
            return palette.GetColor(iter, maxIter);
        }

        private static uint ComputeSumSquared(double x0, double y0, int maxIter, ColorPalette palette, bool smooth)
        {
            double x = 0;
            double y = 0;
            int iter = 0;

            while (x * x + y * y <= 4.0 && iter < maxIter)
            {
                double sum = x + y;
                // (a + b)^2 expansion applied to coordinate transformation
                double nextX = sum * sum + x0;
                double nextY = 2.0 * x * y + y0;
                x = nextX;
                y = nextY;
                iter++;
            }

            if (iter >= maxIter) return 0xFF000000;
            return palette.GetColor(iter, maxIter);
        }

        private static uint ComputeZPlusCSquared(double x0, double y0, int maxIter, ColorPalette palette, bool smooth)
        {
            ComplexNumber z = new(0, 0);
            ComplexNumber c = new(x0, y0);
            int iter = 0;

            while (z.MagnitudeSquared <= 4.0 && iter < maxIter)
            {
                // (z + c)^2 = z^2 + 2zc + c^2
                ComplexNumber sum = z + c;
                z = sum * sum;
                iter++;
            }

            if (iter >= maxIter) return 0xFF000000;
            return palette.GetColor(iter, maxIter);
        }

        private static uint Blend4Colors(uint c0, uint c1, uint c2, uint c3)
        {
            uint b = (((c0 & 0xFF) + (c1 & 0xFF) + (c2 & 0xFF) + (c3 & 0xFF)) >> 2) & 0xFF;
            uint g = (((c0 >> 8 & 0xFF) + (c1 >> 8 & 0xFF) + (c2 >> 8 & 0xFF) + (c3 >> 8 & 0xFF)) >> 2) & 0xFF;
            uint r = (((c0 >> 16 & 0xFF) + (c1 >> 16 & 0xFF) + (c2 >> 16 & 0xFF) + (c3 >> 16 & 0xFF)) >> 2) & 0xFF;
            uint a = 0xFF;
            return (a << 24) | (r << 16) | (g << 8) | b;
        }
    }
}
