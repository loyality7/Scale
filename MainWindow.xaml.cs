using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ScaleWindows.Core;
using ScaleWindows.Models;
using ScaleWindows.Rendering;
using ScaleWindows.Services;

namespace ScaleWindows
{
    public partial class MainWindow : Window
    {
        private readonly RenderParameters _params = new();
        private readonly List<EquationDefinition> _equations = EquationDefinition.GetAll();
        private List<ColorPalette> _palettes = new();
        private ColorPalette _activePalette;

        // Main Viewport
        private WriteableBitmap? _bitmap;
        private int _viewWidth;
        private int _viewHeight;
        private readonly object _renderLock = new();
        private bool _isRendering;
        private bool _isRenderQueued;
        private bool _isSyncing;
        private CancellationTokenSource? _renderCts;
        private CancellationTokenSource? _exportCts;

        // Mouse Drag / Navigation state
        private bool _isDragging;
        private Point _dragStartPoint;
        private double _dragStartCenterX;
        private double _dragStartCenterY;
        private readonly Random _rand = new();

        private readonly List<BookmarkPreset> _landmarks = BookmarkPreset.GetDefaults();
        private bool _isFullscreen;
        private WindowState _prevWindowState;
        private WindowStyle _prevWindowStyle;

        public MainWindow()
        {
            InitializeComponent();

            _palettes = PalettePresets.GetPresets(1.0);
            _activePalette = _palettes[0]; // Electric Cyan & Blue default

            _params.Equation = EquationType.Mandelbrot;
            _params.CenterX = -0.75;
            _params.CenterY = 0.0;
            _params.Zoom = 250.0;
            _params.Power = 2;
            _params.MaxIterations = 200;
            _params.CustomFormula = "(a + b)^2 + c";

            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Populate Landmarks & Reference Presets Dropdown
            if (CmbLandmarks != null)
            {
                CmbLandmarks.ItemsSource = _landmarks;
            }

            // Populate All Equations Dropdown
            CmbAllEquations.ItemsSource = _equations;
            CmbAllEquations.SelectedIndex = 0;

            // Populate All Color Palettes Dropdown (Electric Cyan & Blue is 0)
            CmbPalette.ItemsSource = _palettes;
            CmbPalette.SelectedIndex = 0;

            UpdateCanvasSize();
            SyncInspectorFields();
            QueueRender();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCanvasSize();
            QueueRender();
        }

        private void UpdateCanvasSize()
        {
            int w = (int)ImgViewport.ActualWidth;
            int h = (int)ImgViewport.ActualHeight;

            if (w <= 10 || h <= 10)
            {
                w = Math.Max(100, (int)Width);
                h = Math.Max(100, (int)Height - 54);
            }

            if (w == _viewWidth && h == _viewHeight && _bitmap != null) return;

            _viewWidth = w;
            _viewHeight = h;

            _bitmap = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
            ImgViewport.Source = _bitmap;
        }

        private void QueueRender()
        {
            if (_viewWidth <= 0 || _viewHeight <= 0 || _bitmap == null) return;

            UpdateStatusPill();

            lock (_renderLock)
            {
                // Signal currently running render to cancel so it yields CPU immediately
                _renderCts?.Cancel();
                _isRenderQueued = true;

                if (_isRendering)
                {
                    // A background worker loop is already active; it will pick up the latest queued parameters
                    return;
                }

                _isRendering = true;
            }

            Task.Run(ProcessRenderQueueAsync);
        }

        private async Task ProcessRenderQueueAsync()
        {
            while (true)
            {
                CancellationTokenSource cts;
                RenderParameters currentParams;
                ColorPalette palette;
                int w, h;

                lock (_renderLock)
                {
                    if (!_isRenderQueued)
                    {
                        _isRendering = false;
                        return;
                    }

                    _isRenderQueued = false;
                    _renderCts?.Dispose();
                    _renderCts = new CancellationTokenSource();
                    cts = _renderCts;

                    currentParams = (RenderParameters)_params.Clone();
                    palette = _activePalette;
                    w = _viewWidth;
                    h = _viewHeight;
                }

                uint[] buffer = new uint[w * h];
                CancellationToken token = cts.Token;
                Stopwatch sw = Stopwatch.StartNew();

                bool success;
                if (currentParams.Equation == EquationType.Spirograph ||
                    currentParams.Equation == EquationType.PolarRose ||
                    currentParams.Equation == EquationType.Harmonograph)
                {
                    success = CurveRenderer.Render(currentParams, palette, w, h, buffer, token);
                }
                else
                {
                    success = FractalRenderer.Render(currentParams, palette, w, h, buffer, token);
                }

                sw.Stop();

                if (success && !token.IsCancellationRequested)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        if (token.IsCancellationRequested || _bitmap == null || w != _viewWidth || h != _viewHeight) return;

                        _bitmap.Lock();
                        unsafe
                        {
                            fixed (uint* pBuf = buffer)
                            {
                                Buffer.MemoryCopy(pBuf, (void*)_bitmap.BackBuffer, (long)w * h * 4, (long)w * h * 4);
                            }
                        }
                        _bitmap.AddDirtyRect(new Int32Rect(0, 0, w, h));
                        _bitmap.Unlock();

                        TxtStatusRender.Text = $"{sw.ElapsedMilliseconds} ms";
                    });
                }
            }
        }

        private void UpdateStatusPill()
        {
            if (TxtStatusMode == null || TxtStatusZoom == null) return;

            string title = _params.Equation switch
            {
                EquationType.CustomEquation => $"Equation: {_params.CustomFormula}",
                EquationType.Mandelbrot => "Mandelbrot Set",
                EquationType.Julia => "Julia Set",
                EquationType.SumSquared => "(a + b)² Algebraic",
                EquationType.ZPlusCSquared => "(z + c)² Fractal",
                EquationType.BurningShip => "Burning Ship",
                EquationType.Newton => "Newton-Raphson",
                _ => _params.Equation.ToString()
            };

            TxtStatusMode.Text = title;
            TxtStatusZoom.Text = $"Zoom: {_params.Zoom:N0}x";
        }

        private void SyncInspectorFields()
        {
            if (TxtCenterX == null || TxtCenterY == null ||
                TxtZoomValue == null || TxtDetailValue == null || TxtExponentValue == null ||
                SldZoom == null || SldDetail == null || SldExponent == null ||
                TxtFormulaTitle == null || TxtFormulaDisplay == null)
            {
                return;
            }

            _isSyncing = true;
            try
            {
                TxtCenterX.Text = _params.CenterX.ToString("F6");
                TxtCenterY.Text = _params.CenterY.ToString("F6");
                TxtZoomValue.Text = ((int)_params.Zoom).ToString();
                TxtDetailValue.Text = _params.MaxIterations.ToString();
                TxtExponentValue.Text = _params.Power.ToString();

                SldZoom.Value = Math.Log2(Math.Max(1, _params.Zoom));
                SldDetail.Value = Math.Clamp(_params.MaxIterations, SldDetail.Minimum, SldDetail.Maximum);
                SldExponent.Value = Math.Clamp(_params.Power, SldExponent.Minimum, SldExponent.Maximum);

                EquationDefinition? activeDef = CmbAllEquations?.SelectedItem as EquationDefinition ??
                                                _equations.FirstOrDefault(x => x.Type == _params.Equation);

                if (_params.Equation == EquationType.CustomEquation)
                {
                    var testParser = new CustomExpressionParser(_params.CustomFormula);
                    if (testParser.HasParseError)
                    {
                        TxtFormulaTitle.Text = "Custom Equation (Syntax Warning)";
                        TxtFormulaDisplay.Text = $"⚠️ Could not parse: {_params.CustomFormula} (fallback to z² + c)";
                        TxtFormulaDisplay.Foreground = new SolidColorBrush(Color.FromRgb(255, 180, 0));
                    }
                    else
                    {
                        TxtFormulaTitle.Text = activeDef != null && activeDef.Type == EquationType.CustomEquation ? activeDef.Name : "Custom Dynamic Equation";
                        TxtFormulaDisplay.Text = $"z_{{n+1}} = {_params.CustomFormula}";
                        TxtFormulaDisplay.Foreground = new SolidColorBrush(Color.FromRgb(0, 229, 255));
                    }
                    if (TxtHeaderEquation != null) TxtHeaderEquation.Text = _params.CustomFormula;
                    if (TxtInspectorEquation != null) TxtInspectorEquation.Text = _params.CustomFormula;
                }
                else
                {
                    if (activeDef != null)
                    {
                        TxtFormulaTitle.Text = activeDef.Name;
                        TxtFormulaDisplay.Text = activeDef.FormulaLatex;
                        if (TxtHeaderEquation != null) TxtHeaderEquation.Text = activeDef.Formula;
                        if (TxtInspectorEquation != null) TxtInspectorEquation.Text = activeDef.Formula;

                        if (CmbAllEquations != null && CmbAllEquations.SelectedItem != activeDef)
                        {
                            CmbAllEquations.SelectedItem = activeDef;
                        }
                    }
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }

        // --- CUSTOM EQUATION ENTRY & RANDOMIZATION ---

        private void ApplyCustomEquation(string expr)
        {
            if (string.IsNullOrWhiteSpace(expr)) return;
            expr = expr.Trim();

            _params.CustomFormula = expr;
            _params.Equation = EquationType.CustomEquation;

            // Auto-framing: if user is zoomed deep into another fractal, reset view to full overview
            if (_params.Zoom > 800.0 || Math.Abs(_params.CenterX) > 2.0 || Math.Abs(_params.CenterY) > 2.0)
            {
                _params.CenterX = 0.0;
                _params.CenterY = 0.0;
                _params.Zoom = 250.0;
            }

            if (TxtHeaderEquation != null) TxtHeaderEquation.Text = expr;
            if (TxtInspectorEquation != null) TxtInspectorEquation.Text = expr;

            var testParser = new CustomExpressionParser(expr);
            if (testParser.HasParseError)
            {
                if (TxtFormulaTitle != null) TxtFormulaTitle.Text = "Custom Equation (Syntax Warning)";
                if (TxtFormulaDisplay != null)
                {
                    TxtFormulaDisplay.Text = $"⚠️ Could not parse: {expr} (fallback to z² + c)";
                    TxtFormulaDisplay.Foreground = new SolidColorBrush(Color.FromRgb(255, 180, 0));
                }
            }
            else
            {
                if (TxtFormulaTitle != null) TxtFormulaTitle.Text = "Custom Dynamic Equation";
                if (TxtFormulaDisplay != null)
                {
                    TxtFormulaDisplay.Text = $"z_{{n+1}} = {expr}";
                    TxtFormulaDisplay.Foreground = new SolidColorBrush(Color.FromRgb(0, 229, 255));
                }
            }

            SyncInspectorFields();
            QueueRender();
        }

        private void BtnApplyHeaderEquation_Click(object sender, RoutedEventArgs e)
        {
            ApplyCustomEquation(TxtHeaderEquation.Text);
        }

        private void TxtHeaderEquation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyCustomEquation(TxtHeaderEquation.Text);
            }
        }

        private void BtnApplyInspectorEquation_Click(object sender, RoutedEventArgs e)
        {
            ApplyCustomEquation(TxtInspectorEquation.Text);
        }

        private void TxtInspectorEquation_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyCustomEquation(TxtInspectorEquation.Text);
            }
        }

        private void PickRandomEquation()
        {
            if (_equations.Count == 0) return;

            int currentIndex = CmbAllEquations.SelectedIndex;
            int nextIndex;
            if (_equations.Count > 1)
            {
                do
                {
                    nextIndex = _rand.Next(_equations.Count);
                } while (nextIndex == currentIndex);
            }
            else
            {
                nextIndex = 0;
            }

            CmbAllEquations.SelectedIndex = nextIndex;
        }

        private void BtnRandEquation_Click(object sender, RoutedEventArgs e)
        {
            PickRandomEquation();
        }

        private void CmbAllEquations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (CmbAllEquations.SelectedItem is EquationDefinition def)
            {
                _params.Equation = def.Type;
                if (!string.IsNullOrWhiteSpace(def.Formula))
                {
                    _params.CustomFormula = def.Formula;
                }
                else if (!string.IsNullOrWhiteSpace(def.FormulaLatex))
                {
                    _params.CustomFormula = def.FormulaLatex;
                }

                if (def.DefaultZoom > 0)
                {
                    _params.CenterX = def.DefaultCenterX;
                    _params.CenterY = def.DefaultCenterY;
                    _params.Zoom = def.DefaultZoom;
                    _params.Power = def.DefaultPower > 0 ? def.DefaultPower : 2;
                    _params.MaxIterations = def.DefaultMaxIterations > 0 ? def.DefaultMaxIterations : 200;
                }

                if (def.JuliaX != 0 || def.JuliaY != 0)
                {
                    _params.JuliaX = def.JuliaX;
                    _params.JuliaY = def.JuliaY;
                }

                if (TxtHeaderEquation != null) TxtHeaderEquation.Text = _params.CustomFormula;
                if (TxtInspectorEquation != null) TxtInspectorEquation.Text = _params.CustomFormula;

                SyncInspectorFields();
                QueueRender();
            }
        }

        // --- TOP TOOLBAR ACTIONS ---

        private void BtnRandomize_Click(object sender, RoutedEventArgs e)
        {
            PickRandomEquation();
        }

        private void BtnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            _params.Zoom = Math.Min(50_000_000.0, _params.Zoom * 1.25);
            SyncInspectorFields();
            QueueRender();
        }

        private void BtnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            _params.Zoom = Math.Max(20.0, _params.Zoom / 1.25);
            SyncInspectorFields();
            QueueRender();
        }

        private void BtnResetView_Click(object sender, RoutedEventArgs e)
        {
            ResetView();
        }

        private void ResetView()
        {
            if (_params.Equation == EquationType.CustomEquation)
            {
                _params.CenterX = 0.0;
                _params.CenterY = 0.0;
                _params.Zoom = 250.0;
            }
            else
            {
                var def = _equations.FirstOrDefault(x => x.Type == _params.Equation);
                if (def != null)
                {
                    _params.CenterX = def.DefaultCenterX;
                    _params.CenterY = def.DefaultCenterY;
                    _params.Zoom = def.DefaultZoom;
                }
                else
                {
                    _params.CenterX = 0.0;
                    _params.CenterY = 0.0;
                    _params.Zoom = 250.0;
                }
            }

            SyncInspectorFields();
            QueueRender();
        }

        private void TglInspect_Click(object sender, RoutedEventArgs e)
        {
            PnlInspector.Visibility = TglInspect.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new()
            {
                Filter = "PNG Image (*.png)|*.png",
                DefaultExt = ".png",
                AddExtension = true,
                FileName = $"Scale_{_params.Equation}_{DateTime.Now:yyyyMMdd_HHmmss}.png"
            };

            if (dlg.ShowDialog() != true) return;

            string destPath = dlg.FileName;
            string fileNameOnly = System.IO.Path.GetFileName(destPath);

            // Setup Export Overlay UI
            if (TxtExportSubtitle != null)
            {
                TxtExportSubtitle.Text = $"Rendering 4K UHD (3840 × 2160) • {TxtFormulaTitle?.Text ?? _params.Equation.ToString()}";
            }
            if (TxtExportFilename != null)
            {
                TxtExportFilename.Text = fileNameOnly;
            }
            if (PrgExport != null) PrgExport.Value = 0;
            if (TxtExportPercent != null) TxtExportPercent.Text = "0%";
            if (TxtExportStatus != null) TxtExportStatus.Text = "Starting multi-threaded 4K render...";
            if (OverlayExportProgress != null) OverlayExportProgress.Visibility = Visibility.Visible;

            _exportCts?.Cancel();
            _exportCts = new CancellationTokenSource();
            CancellationToken ct = _exportCts.Token;

            var progress = new Progress<double>(percent =>
            {
                if (PrgExport != null) PrgExport.Value = percent;
                if (TxtExportPercent != null) TxtExportPercent.Text = $"{(int)percent}%";
                if (TxtExportStatus != null)
                {
                    if (percent >= 96.0)
                    {
                        TxtExportStatus.Text = "Encoding high-resolution PNG image...";
                    }
                    else
                    {
                        TxtExportStatus.Text = $"Rendering 4K scanlines... {(int)percent}%";
                    }
                }
            });

            try
            {
                TxtStatusRender.Text = "Exporting 4K...";
                bool success = await ImageExporter.ExportToFileAsync(
                    destPath,
                    3840,
                    2160,
                    _params,
                    _activePalette,
                    _viewWidth,
                    progress,
                    ct);

                if (OverlayExportProgress != null) OverlayExportProgress.Visibility = Visibility.Collapsed;

                if (success)
                {
                    TxtStatusRender.Text = "4K Saved ✓";
                    MessageBox.Show($"4K Ultra HD Wallpaper exported successfully:\n\n{destPath}", "Image Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (ct.IsCancellationRequested)
                {
                    TxtStatusRender.Text = "Export cancelled.";
                }
                else
                {
                    TxtStatusRender.Text = "Save failed.";
                    MessageBox.Show("Could not save image file. Please check folder write permissions.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (OperationCanceledException)
            {
                if (OverlayExportProgress != null) OverlayExportProgress.Visibility = Visibility.Collapsed;
                TxtStatusRender.Text = "Export cancelled.";
            }
            catch (Exception ex)
            {
                if (OverlayExportProgress != null) OverlayExportProgress.Visibility = Visibility.Collapsed;
                TxtStatusRender.Text = "Export error.";
                MessageBox.Show($"Export error: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (OverlayExportProgress != null) OverlayExportProgress.Visibility = Visibility.Collapsed;
                _exportCts?.Dispose();
                _exportCts = null;
            }
        }

        private void BtnCancelExport_Click(object sender, RoutedEventArgs e)
        {
            if (TxtExportStatus != null) TxtExportStatus.Text = "Cancelling export...";
            _exportCts?.Cancel();
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_bitmap != null)
                {
                    Clipboard.SetImage(_bitmap);
                    TxtStatusRender.Text = "Image copied to clipboard!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not copy image to clipboard: {ex.Message}", "Clipboard Notice", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCopyCoords_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string info = $"Scale Coordinates:\nEquation: {_params.Equation}\nFormula: {_params.CustomFormula}\nCenter: ({_params.CenterX:F8}, {_params.CenterY:F8})\nZoom: {_params.Zoom:N0}x\nDetail: {_params.MaxIterations}\nPower: {_params.Power}\nJulia: ({_params.JuliaX:F6}, {_params.JuliaY:F6})";
                Clipboard.SetText(info);
                TxtStatusRender.Text = "Coordinates copied to clipboard!";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not copy coordinates to clipboard: {ex.Message}", "Clipboard Notice", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CmbLandmarks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (CmbLandmarks.SelectedItem is BookmarkPreset preset)
            {
                _params.Equation = preset.Equation;
                _params.CenterX = preset.CenterX;
                _params.CenterY = preset.CenterY;
                _params.Zoom = preset.Zoom;
                _params.Power = preset.Power > 0 ? preset.Power : 2;
                _params.MaxIterations = preset.MaxIterations > 0 ? preset.MaxIterations : 200;
                _params.JuliaX = preset.JuliaX;
                _params.JuliaY = preset.JuliaY;

                var def = _equations.FirstOrDefault(x => x.Type == preset.Equation);
                if (def != null && !string.IsNullOrWhiteSpace(def.Formula))
                {
                    _params.CustomFormula = def.Formula;
                }

                if (!string.IsNullOrWhiteSpace(preset.SuggestedPalette))
                {
                    var matchPal = _palettes.FirstOrDefault(p => p.Name.Contains(preset.SuggestedPalette, StringComparison.OrdinalIgnoreCase));
                    if (matchPal != null)
                    {
                        CmbPalette.SelectedItem = matchPal;
                        _activePalette = matchPal;
                    }
                }

                SyncInspectorFields();
                QueueRender();
            }
        }

        private void BtnRandLandmark_Click(object sender, RoutedEventArgs e)
        {
            if (_landmarks.Count == 0) return;
            int nextIdx;
            do
            {
                nextIdx = _rand.Next(_landmarks.Count);
            } while (_landmarks.Count > 1 && nextIdx == CmbLandmarks.SelectedIndex);

            CmbLandmarks.SelectedIndex = nextIdx;
        }

        // --- INSPECTOR CONTROLS ---

        private void CommitCenterCoordinates()
        {
            if (double.TryParse(TxtCenterX.Text, out double cx)) _params.CenterX = cx;
            if (double.TryParse(TxtCenterY.Text, out double cy)) _params.CenterY = cy;
            QueueRender();
        }

        private void TxtCenter_LostFocus(object sender, RoutedEventArgs e)
        {
            CommitCenterCoordinates();
        }

        private void TxtCenter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitCenterCoordinates();
            }
        }

        private void SldZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isSyncing || TxtZoomValue == null) return;
            _params.Zoom = Math.Clamp(Math.Pow(2, e.NewValue), 20.0, 50_000_000.0);
            TxtZoomValue.Text = ((int)_params.Zoom).ToString();
            QueueRender();
        }

        private void CommitZoomValue()
        {
            if (double.TryParse(TxtZoomValue.Text, out double z) && z > 0)
            {
                _params.Zoom = Math.Clamp(z, 20.0, 50_000_000.0);
                SldZoom.Value = Math.Log2(_params.Zoom);
                QueueRender();
            }
        }

        private void TxtZoomValue_LostFocus(object sender, RoutedEventArgs e)
        {
            CommitZoomValue();
        }

        private void TxtZoomValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitZoomValue();
            }
        }

        private void SldDetail_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isSyncing || TxtDetailValue == null) return;
            _params.MaxIterations = (int)e.NewValue;
            TxtDetailValue.Text = _params.MaxIterations.ToString();
            QueueRender();
        }

        private void CommitDetailValue()
        {
            if (int.TryParse(TxtDetailValue.Text, out int d) && d > 5)
            {
                _params.MaxIterations = d;
                SldDetail.Value = Math.Clamp(d, SldDetail.Minimum, SldDetail.Maximum);
                QueueRender();
            }
        }

        private void TxtDetailValue_LostFocus(object sender, RoutedEventArgs e)
        {
            CommitDetailValue();
        }

        private void TxtDetailValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitDetailValue();
            }
        }

        private void SldExponent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isSyncing || TxtExponentValue == null) return;
            _params.Power = (int)e.NewValue;
            TxtExponentValue.Text = _params.Power.ToString();
            QueueRender();
        }

        private void CommitExponentValue()
        {
            if (int.TryParse(TxtExponentValue.Text, out int p) && p >= 1 && p <= 12)
            {
                _params.Power = p;
                SldExponent.Value = Math.Clamp(p, SldExponent.Minimum, SldExponent.Maximum);
                QueueRender();
            }
        }

        private void TxtExponentValue_LostFocus(object sender, RoutedEventArgs e)
        {
            CommitExponentValue();
        }

        private void TxtExponentValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitExponentValue();
            }
        }

        // --- COLORS PANEL ---

        private void CmbPalette_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (CmbPalette.SelectedItem is ColorPalette pal)
            {
                _activePalette = pal;
                if (BrdGradientPreview != null)
                {
                    BrdGradientPreview.Background = pal.PreviewBrush;
                }
                QueueRender();
            }
        }

        private void SldGamma_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtGammaDisplay == null) return;
            _params.Gamma = e.NewValue;
            TxtGammaDisplay.Text = $"{_params.Gamma:F2}";

            int idx = Math.Max(0, CmbPalette?.SelectedIndex ?? 0);
            _palettes = PalettePresets.GetPresets(_params.Gamma);
            if (CmbPalette != null)
            {
                CmbPalette.ItemsSource = _palettes;
                CmbPalette.SelectedIndex = Math.Min(idx, _palettes.Count - 1);
                if (CmbPalette.SelectedItem is ColorPalette selected)
                {
                    _activePalette = selected;
                    if (BrdGradientPreview != null) BrdGradientPreview.Background = _activePalette.PreviewBrush;
                }
            }
            QueueRender();
        }

        private void ChkRendering_Changed(object sender, RoutedEventArgs e)
        {
            _params.SmoothColoring = ChkSmooth?.IsChecked ?? true;
            QueueRender();
        }

        private void CmbAntialiasing_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbAntialiasing == null) return;
            _params.Antialiasing = CmbAntialiasing.SelectedIndex == 1 ? 2 : 1;
            QueueRender();
        }

        // --- MOUSE CANVAS INTERACTION ---

        private void ImgViewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                ResetView();
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.ClickCount == 2)
                {
                    Point pt = e.GetPosition(ImgViewport);
                    ZoomAtPoint(pt, 1.35);
                }
                else
                {
                    _isDragging = true;
                    _dragStartPoint = e.GetPosition(ImgViewport);
                    _dragStartCenterX = _params.CenterX;
                    _dragStartCenterY = _params.CenterY;
                    ImgViewport.CaptureMouse();
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                Point pt = e.GetPosition(ImgViewport);
                ZoomAtPoint(pt, 1.0 / 1.25);
            }
        }

        private void ImgViewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPt = e.GetPosition(ImgViewport);
                double dx = currentPt.X - _dragStartPoint.X;
                double dy = currentPt.Y - _dragStartPoint.Y;

                _params.CenterX = _dragStartCenterX - (dx / _params.Zoom);
                _params.CenterY = _dragStartCenterY + (dy / _params.Zoom);

                TxtCenterX.Text = _params.CenterX.ToString("F6");
                TxtCenterY.Text = _params.CenterY.ToString("F6");

                QueueRender();
            }
        }

        private void ImgViewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                ImgViewport.ReleaseMouseCapture();
            }
        }

        private void ImgViewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point pt = e.GetPosition(ImgViewport);
            double factor = e.Delta > 0 ? 1.15 : (1.0 / 1.15);
            ZoomAtPoint(pt, factor);
        }

        private void ZoomAtPoint(Point screenPoint, double factor)
        {
            double halfW = _viewWidth * 0.5;
            double halfH = _viewHeight * 0.5;

            double worldX = _params.CenterX + (screenPoint.X - halfW) / _params.Zoom;
            double worldY = _params.CenterY + (halfH - screenPoint.Y) / _params.Zoom;

            _params.Zoom = Math.Clamp(_params.Zoom * factor, 20.0, 50_000_000.0);

            _params.CenterX = worldX - (screenPoint.X - halfW) / _params.Zoom;
            _params.CenterY = worldY - (halfH - screenPoint.Y) / _params.Zoom;

            SyncInspectorFields();
            QueueRender();
        }

        // --- KEYBOARD SHORTCUTS & DESKTOP ACCELERATORS ---

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool isCtrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            bool isShift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            bool isTextBox = Keyboard.FocusedElement is TextBox;

            // Global Accelerators with Ctrl
            if (isCtrl)
            {
                if (isShift && e.Key == Key.C)
                {
                    BtnCopyCoords_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.S)
                {
                    BtnSave_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.C && !isTextBox)
                {
                    BtnCopy_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.R)
                {
                    ResetView();
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.I)
                {
                    TglInspect.IsChecked = !TglInspect.IsChecked;
                    TglInspect_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    return;
                }
            }

            if (e.Key == Key.F11)
            {
                ToggleFullscreen();
                e.Handled = true;
                return;
            }

            // Hotkeys when not editing text in an input box
            if (!isTextBox)
            {
                if (e.Key == Key.Home)
                {
                    ResetView();
                    e.Handled = true;
                }
                else if (e.Key == Key.Space)
                {
                    PickRandomEquation();
                    e.Handled = true;
                }
                else if (e.Key == Key.OemPlus || e.Key == Key.Add)
                {
                    BtnZoomIn_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                }
                else if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
                {
                    BtnZoomOut_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                }
                else if (e.Key == Key.Left)
                {
                    PanByPixels(50, 0);
                    e.Handled = true;
                }
                else if (e.Key == Key.Right)
                {
                    PanByPixels(-50, 0);
                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    PanByPixels(0, -50);
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    PanByPixels(0, 50);
                    e.Handled = true;
                }
            }
            else
            {
                if (e.Key == Key.Escape)
                {
                    ImgViewport.Focus();
                    Keyboard.ClearFocus();
                    e.Handled = true;
                }
            }
        }

        private void PanByPixels(double dx, double dy)
        {
            _params.CenterX -= dx / _params.Zoom;
            _params.CenterY += dy / _params.Zoom;
            TxtCenterX.Text = _params.CenterX.ToString("F6");
            TxtCenterY.Text = _params.CenterY.ToString("F6");
            QueueRender();
        }

        private void ToggleFullscreen()
        {
            if (!_isFullscreen)
            {
                _prevWindowState = WindowState;
                _prevWindowStyle = WindowStyle;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                _isFullscreen = true;
            }
            else
            {
                WindowStyle = _prevWindowStyle;
                WindowState = _prevWindowState;
                _isFullscreen = false;
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            lock (_renderLock)
            {
                _isRenderQueued = false;
                _renderCts?.Cancel();
                _renderCts?.Dispose();
                _renderCts = null;
            }

            _exportCts?.Cancel();
            _exportCts?.Dispose();
            _exportCts = null;
        }
    }
}