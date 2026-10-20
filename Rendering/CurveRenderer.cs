using System;
using System.Threading;
using ScaleWindows.Core;
using ScaleWindows.Models;

namespace ScaleWindows.Rendering
{
    public class CurveRenderer
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

            // Clear to background (Dark Charcoal/Black)
            Array.Fill(pixelBuffer, 0xFF0D0E15);

            double zoom = parameters.Zoom;
            double centerX = parameters.CenterX;
            double centerY = parameters.CenterY;
            double halfW = width * 0.5;
            double halfH = height * 0.5;

            switch (parameters.Equation)
            {
                case EquationType.Spirograph:
                    RenderSpirograph(parameters, palette, width, height, halfW, halfH, zoom, centerX, centerY, pixelBuffer, cancellationToken);
                    break;
                case EquationType.PolarRose:
                    RenderPolarRose(parameters, palette, width, height, halfW, halfH, zoom, centerX, centerY, pixelBuffer, cancellationToken);
                    break;
                case EquationType.Harmonograph:
                    RenderHarmonograph(parameters, palette, width, height, halfW, halfH, zoom, centerX, centerY, pixelBuffer, cancellationToken);
                    break;
            }

            progress?.Report(95.0);
            return true;
        }

        private static void RenderSpirograph(
            RenderParameters p, ColorPalette pal, int w, int h, double hw, double hh, double zoom, double cx, double cy,
            uint[] buf, CancellationToken ct)
        {
            double R = p.CurveParamR;
            double r = p.CurveParamLittleR;
            double d = p.CurveParamD;
            int totalSteps = 120000;
            double step = 0.005;

            double prevPx = 0, prevPy = 0;
            bool first = true;

            for (int i = 0; i < totalSteps; i++)
            {
                if (ct.IsCancellationRequested) return;
                double theta = i * step;
                double x = (R - r) * Math.Cos(theta) + d * Math.Cos((R - r) * theta / r);
                double y = (R - r) * Math.Sin(theta) - d * Math.Sin((R - r) * theta / r);

                double sx = hw + (x - cx) * zoom * 0.15;
                double sy = hh - (y - cy) * zoom * 0.15;

                double t = (double)i / totalSteps;
                uint color = pal.GetDirectColor(t);

                if (!first)
                {
                    DrawLine(buf, w, h, (int)prevPx, (int)prevPy, (int)sx, (int)sy, color);
                }
                prevPx = sx;
                prevPy = sy;
                first = false;
            }
        }

        private static void RenderPolarRose(
            RenderParameters p, ColorPalette pal, int w, int h, double hw, double hh, double zoom, double cx, double cy,
            uint[] buf, CancellationToken ct)
        {
            double k = p.CurveRoseK > 0 ? p.CurveRoseK : 7.0;
            double a = 1.0;
            int totalSteps = 80000;
            double maxTheta = Math.PI * 16.0;
            double step = maxTheta / totalSteps;

            double prevPx = 0, prevPy = 0;
            bool first = true;

            for (int i = 0; i < totalSteps; i++)
            {
                if (ct.IsCancellationRequested) return;
                double theta = i * step;
                double r = a * Math.Cos(k * theta);
                double x = r * Math.Cos(theta);
                double y = r * Math.Sin(theta);

                double sx = hw + (x - cx) * zoom;
                double sy = hh - (y - cy) * zoom;

                double t = (double)i / totalSteps;
                uint color = pal.GetDirectColor(t);

                if (!first)
                {
                    DrawLine(buf, w, h, (int)prevPx, (int)prevPy, (int)sx, (int)sy, color);
                }
                prevPx = sx;
                prevPy = sy;
                first = false;
            }
        }

        private static void RenderHarmonograph(
            RenderParameters p, ColorPalette pal, int w, int h, double hw, double hh, double zoom, double cx, double cy,
            uint[] buf, CancellationToken ct)
        {
            double f1 = 2.001, f2 = 3.0, f3 = 3.002, f4 = 2.0;
            double d1 = 0.0008, d2 = 0.001, d3 = 0.001, d4 = 0.0008;
            double p1 = Math.PI / 16, p2 = 3 * Math.PI / 2, p3 = Math.PI / 2, p4 = 0;

            int totalSteps = 150000;
            double dt = 0.02;

            double prevPx = 0, prevPy = 0;
            bool first = true;

            for (int i = 0; i < totalSteps; i++)
            {
                if (ct.IsCancellationRequested) return;
                double t = i * dt;
                double x = Math.Sin(f1 * t + p1) * Math.Exp(-d1 * t) + Math.Sin(f2 * t + p2) * Math.Exp(-d2 * t);
                double y = Math.Sin(f3 * t + p3) * Math.Exp(-d3 * t) + Math.Sin(f4 * t + p4) * Math.Exp(-d4 * t);

                double sx = hw + (x - cx) * zoom * 0.7;
                double sy = hh - (y - cy) * zoom * 0.7;

                double colorT = (double)i / totalSteps;
                uint color = pal.GetDirectColor(colorT);

                if (!first)
                {
                    DrawLine(buf, w, h, (int)prevPx, (int)prevPy, (int)sx, (int)sy, color);
                }
                prevPx = sx;
                prevPy = sy;
                first = false;
            }
        }

        // Fast Bresenham's line algorithm
        private static void DrawLine(uint[] buf, int w, int h, int x0, int y0, int x1, int y1, uint color)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (x0 >= 0 && x0 < w && y0 >= 0 && y0 < h)
                {
                    buf[y0 * w + x0] = color;
                }

                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
    }
}
