using System;

namespace ScaleWindows.Core
{
    /// <summary>
    /// High-performance value-type struct for complex number mathematics.
    /// Supports basic arithmetic, powers, trigonometric and exponential operations.
    /// </summary>
    public readonly struct ComplexNumber
    {
        public readonly double Real;
        public readonly double Imaginary;

        public ComplexNumber(double real, double imaginary)
        {
            Real = real;
            Imaginary = imaginary;
        }

        public double MagnitudeSquared => Real * Real + Imaginary * Imaginary;
        public double Magnitude => Math.Sqrt(MagnitudeSquared);

        public static ComplexNumber operator +(ComplexNumber a, ComplexNumber b) =>
            new(a.Real + b.Real, a.Imaginary + b.Imaginary);

        public static ComplexNumber operator -(ComplexNumber a, ComplexNumber b) =>
            new(a.Real - b.Real, a.Imaginary - b.Imaginary);

        public static ComplexNumber operator *(ComplexNumber a, ComplexNumber b) =>
            new(a.Real * b.Real - a.Imaginary * b.Imaginary,
                a.Real * b.Imaginary + a.Imaginary * b.Real);

        public static ComplexNumber operator *(ComplexNumber a, double scalar) =>
            new(a.Real * scalar, a.Imaginary * scalar);

        public static ComplexNumber operator /(ComplexNumber a, ComplexNumber b)
        {
            double den = b.Real * b.Real + b.Imaginary * b.Imaginary;
            if (den == 0) return new ComplexNumber(0, 0);
            return new ComplexNumber(
                (a.Real * b.Real + a.Imaginary * b.Imaginary) / den,
                (a.Imaginary * b.Real - a.Real * b.Imaginary) / den
            );
        }

        public static ComplexNumber operator -(ComplexNumber a) =>
            new(-a.Real, -a.Imaginary);

        public static ComplexNumber operator +(ComplexNumber a, double b) =>
            new(a.Real + b, a.Imaginary);

        public static ComplexNumber operator +(double a, ComplexNumber b) =>
            new(a + b.Real, b.Imaginary);

        public static ComplexNumber operator -(ComplexNumber a, double b) =>
            new(a.Real - b, a.Imaginary);

        public static ComplexNumber operator -(double a, ComplexNumber b) =>
            new(a - b.Real, -b.Imaginary);

        public static ComplexNumber operator *(double scalar, ComplexNumber a) =>
            new(a.Real * scalar, a.Imaginary * scalar);

        public static ComplexNumber operator /(ComplexNumber a, double scalar) =>
            scalar == 0 ? new(0, 0) : new(a.Real / scalar, a.Imaginary / scalar);

        public static ComplexNumber operator /(double scalar, ComplexNumber b)
        {
            double den = b.Real * b.Real + b.Imaginary * b.Imaginary;
            if (den == 0) return new ComplexNumber(0, 0);
            return new ComplexNumber((scalar * b.Real) / den, (-scalar * b.Imaginary) / den);
        }

        public ComplexNumber Conjugate() => new(Real, -Imaginary);

        public ComplexNumber AbsComponents() => new(Math.Abs(Real), Math.Abs(Imaginary));

        public ComplexNumber Pow(int power)
        {
            if (power == 0) return new ComplexNumber(1, 0);
            if (power < 0) return new ComplexNumber(1, 0) / Pow(-power);
            if (power == 1) return this;
            if (power == 2) return new ComplexNumber(Real * Real - Imaginary * Imaginary, 2 * Real * Imaginary);

            ComplexNumber result = this;
            ComplexNumber current = this;
            for (int i = 1; i < power; i++)
            {
                result *= current;
            }
            return result;
        }

        public ComplexNumber Pow(double power)
        {
            if (power == 1.0) return this;
            if (power == 2.0) return new ComplexNumber(Real * Real - Imaginary * Imaginary, 2 * Real * Imaginary);
            if (Math.Abs(power - Math.Round(power)) < 1e-9 && power > 0 && power <= 10)
            {
                return Pow((int)Math.Round(power));
            }

            double r = Magnitude;
            if (r == 0) return new ComplexNumber(0, 0);
            double theta = Math.Atan2(Imaginary, Real);
            double newR = Math.Pow(r, power);
            return new ComplexNumber(newR * Math.Cos(power * theta), newR * Math.Sin(power * theta));
        }

        public ComplexNumber Sqrt()
        {
            double r = Magnitude;
            double u = Math.Sqrt((r + Real) * 0.5);
            double v = Math.CopySign(Math.Sqrt(Math.Max(0, (r - Real) * 0.5)), Imaginary);
            return new ComplexNumber(u, v);
        }

        public ComplexNumber Sin()
        {
            // sin(x + iy) = sin(x)cosh(y) + i cos(x)sinh(y)
            return new ComplexNumber(
                Math.Sin(Real) * Math.Cosh(Imaginary),
                Math.Cos(Real) * Math.Sinh(Imaginary)
            );
        }

        public ComplexNumber Cos()
        {
            // cos(x + iy) = cos(x)cosh(y) - i sin(x)sinh(y)
            return new ComplexNumber(
                Math.Cos(Real) * Math.Cosh(Imaginary),
                -Math.Sin(Real) * Math.Sinh(Imaginary)
            );
        }

        public ComplexNumber Exp()
        {
            // exp(x + iy) = e^x * (cos(y) + i sin(y))
            double expX = Math.Exp(Real);
            return new ComplexNumber(
                expX * Math.Cos(Imaginary),
                expX * Math.Sin(Imaginary)
            );
        }

        public ComplexNumber Sinh()
        {
            // sinh(x + iy) = sinh(x)cos(y) + i cosh(x)sin(y)
            return new ComplexNumber(
                Math.Sinh(Real) * Math.Cos(Imaginary),
                Math.Cosh(Real) * Math.Sin(Imaginary)
            );
        }

        public ComplexNumber Cosh()
        {
            // cosh(x + iy) = cosh(x)cos(y) + i sinh(x)sin(y)
            return new ComplexNumber(
                Math.Cosh(Real) * Math.Cos(Imaginary),
                Math.Sinh(Real) * Math.Sin(Imaginary)
            );
        }

        public ComplexNumber Tan()
        {
            return Sin() / Cos();
        }

        public ComplexNumber Log()
        {
            double r = Magnitude;
            if (r == 0) return new ComplexNumber(-20, 0);
            return new ComplexNumber(Math.Log(r), Math.Atan2(Imaginary, Real));
        }
    }
}
