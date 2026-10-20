# Scale (Windows Edition) — Real Mathematical Art Generator

A high-performance Windows desktop application built with **C# and .NET 10 (WPF)** for exploring, manipulating, and exporting **real mathematical equation art**.

Ported from Michael Fogleman's macOS project and expanded to support multiple families of real deterministic equations (fractals, root-finding dynamics, transcendental trigonometric forms, and geometric roulette curves).

---

## Supported Mathematical Equations

| Equation | Category | Mathematical Formula | Visual Characteristics |
| :--- | :--- | :--- | :--- |
| **Mandelbrot Set** | Classical Fractal | $z_{n+1} = z_n^p + c$ | Iconic fractal cardioid, circular bulbs, spiral seahorse filaments. |
| **Julia Set** | Classical Fractal | $z_{n+1} = z_n^p + J$ | Infinitely self-similar spirals parameterized by complex seed constant $J$. |
| **Burning Ship** | Fractal | $z_{n+1} = (\|\text{Re}(z)\| + i\|\text{Im}(z)\|)^p + c$ | Gothic ship masts, antennas, and flame textures. |
| **Newton-Raphson** | Root Dynamics | $z_{n+1} = z_n - \frac{z_n^p - 1}{p \cdot z_n^{p-1}}$ | Symmetrical basins of attraction converging to roots of unity. |
| **Tricorn (Mandelbar)** | Antiholomorphic | $z_{n+1} = (\bar{z}_n)^p + c$ | Distinctive 3-cornered crowns formed by complex conjugation. |
| **Transcendental Sine** | Complex Trigonometric | $z_{n+1} = c \cdot \sin(z_n)$ | Organic woven ribbons, periodic latticework, and cellular webs. |
| **Collatz Fractal** | Number Theory | $f(z) = \frac{1}{4}(1 + 4z - (1 + 2z)\cos(\pi z))$ | Complex analytic continuation of the $3n + 1$ conjecture. |
| **Spirograph (Hypotrochoid)** | Roulette Curves | $x(\theta) = (R-r)\cos\theta + d\cos((R-r)\theta/r)$ | Precise geometric curves formed by rolling circles. |
| **Polar Rose (Rhodonea)** | Sinusoidal Geometry | $r(\theta) = a \cdot \cos(k\theta)$ | Petaled harmonic roses studied by Guido Grandi. |
| **Harmonograph** | Resonance Physics | $x(t) = A_1\sin(f_1 t + p_1)e^{-d_1 t}$ | Dual-pendulum mechanical resonance and musical decay harmonics. |

---

## Key Features

* **Multi-Core Acceleration**: Multi-threaded parallel rendering (`Parallel.For`) across all available CPU cores.
* **Dynamic Math AST Parser**: Live evaluation of arbitrary mathematical dynamics $z_{n+1} = f(z, c)$ with full support for:
  * Functions: `sin()`, `cos()`, `tan()`, `sinh()`, `cosh()`, `exp()`, `sqrt()`, `abs()`, `log()`, `ln()`, `conj()`, `bar()`.
  * Implicit multiplication: `2z`, `2(a+b)`, `i|y|`, `2xy`, `(a+b)(a-b)`.
  * Modulus syntax: `|x*y|`, `|z|`.
  * Real & Imaginary components: `a`/`x` ($\text{Re}(z)$), `b`/`y` ($\text{Im}(z)$), `x0`/`cx` ($\text{Re}(c)$), `y0`/`cy` ($\text{Im}(c)$).
* **Curated Landmarks & Reference Presets**: 1-click teleportation to Michael Fogleman's 9 original macOS Scale reference landmarks and classical chaos locations:
  * Quad-Spiral Julia, Deep Spiral Filigree, Scepter Valley Satellite, Cubic Star, Cubic Lattice, Twin Lightning, Pentagonal Star, Cubic Needle, Elephant Valley, Seahorse Valley, Dragon Flame, and more.
* **Interactive Desktop Usability & Hotkeys**:
  * `Left Click + Drag`: Fluid camera pan.
  * `Mouse Wheel`: Smooth exponential zoom centered at mouse cursor.
  * `Ctrl + S`: Save 4K UHD wallpaper directly to disk.
  * `Ctrl + C`: Instant copy of current fractal render to Windows clipboard.
  * `Ctrl + Shift + C`: Instant copy of coordinates & formula to clipboard.
  * `Ctrl + R` / `Home`: Reset view to default framing.
  * `Ctrl + I`: Toggle Inspector & Controls sidebar.
  * `F11`: Toggle borderless immersive fullscreen view.
  * `Space`: Randomize equation / landmark.
  * `+` / `-`: Zoom in / out.
  * `Arrow Keys`: Pan Left, Right, Up, Down.
* **Aesthetic Controls**:
  * **Power / Exponent Slider ($p$)**: Real-time morphing of higher-order symmetry.
  * **Iteration Depth Slider**: Control depth of resolution ($30$ to $600+$).
  * **Curated Color Palettes**: Electric Cyan & Blue, Warm Amber, Emerald Matrix, Cyberpunk Neon, Obsidian Gold, Deep Ocean Abyss, Sunset Coral, Inferno Fire, etc.
  * **Live Gamma Curve Slider**: Continuous contrast and color spectrum curve adjustment.
  * **Supersampling (2x Antialiasing)**: Sub-pixel rendering for razor-sharp boundaries.
* **Enterprise Stability**:
  * Global unhandled exception logging to `crash.log`.
  * Clean thread cancellation and disposal on window close.
  * Native multi-resolution icon (`app.ico`) and assembly branding.

---

## How to Run & Build

### Development Mode
```powershell
dotnet run
```

### Build Release
```powershell
dotnet build -c Release
```

### Publish Standalone Single-File Portable Executable
To create a standalone portable `.exe` that runs on any Windows machine without requiring .NET installed:
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```
The output will be placed in `bin\Release\net10.0-windows\win-x64\publish\ScaleWindows.exe`.
