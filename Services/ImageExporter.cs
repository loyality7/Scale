using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ScaleWindows.Core;
using ScaleWindows.Models;
using ScaleWindows.Rendering;

namespace ScaleWindows.Services
{
    public class ImageExporter
    {
        public static async Task<bool> ExportToFileAsync(
            string filePath,
            int width,
            int height,
            RenderParameters parameters,
            ColorPalette palette,
            double currentViewWidth = 1000.0,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                try
                {
                    progress?.Report(0.0);

                    uint[] buffer = new uint[width * height];
                    RenderParameters exportParams = (RenderParameters)parameters.Clone();

                    // Adjust zoom proportionally to export canvas size vs viewport
                    double viewW = Math.Max(100.0, currentViewWidth);
                    double scaleFactor = (double)width / viewW;
                    exportParams.Zoom *= scaleFactor;
                    exportParams.Antialiasing = 2; // High-quality 2x supersampling on export

                    bool success;
                    if (parameters.Equation == EquationType.Spirograph ||
                        parameters.Equation == EquationType.PolarRose ||
                        parameters.Equation == EquationType.Harmonograph)
                    {
                        success = CurveRenderer.Render(exportParams, palette, width, height, buffer, cancellationToken, progress);
                    }
                    else
                    {
                        success = FractalRenderer.Render(exportParams, palette, width, height, buffer, cancellationToken, progress);
                    }

                    if (!success || cancellationToken.IsCancellationRequested)
                    {
                        TryDeletePartialFile(filePath);
                        return false;
                    }

                    progress?.Report(96.0); // 96%: Rendering finished, encoding PNG

                    // Direct thread-safe buffer encoding to PNG (no UI Dispatcher required)
                    int stride = width * 4;
                    BitmapSource bmpSource = BitmapSource.Create(
                        width,
                        height,
                        96,
                        96,
                        PixelFormats.Bgra32,
                        null,
                        buffer,
                        stride);

                    bmpSource.Freeze();

                    PngBitmapEncoder encoder = new();
                    encoder.Frames.Add(BitmapFrame.Create(bmpSource));

                    using (FileStream stream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        encoder.Save(stream);
                    }

                    progress?.Report(100.0);
                    return true;
                }
                catch (OperationCanceledException)
                {
                    TryDeletePartialFile(filePath);
                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Export error: {ex}");
                    TryDeletePartialFile(filePath);
                    return false;
                }
            }, cancellationToken);
        }

        private static void TryDeletePartialFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            catch
            {
            }
        }
    }
}
