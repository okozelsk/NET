using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.MathTools.MatrixMath
{
    /// <summary>
    /// Eigenvalues and eigenvectors of a real matrix.
    /// This class is based on a class from the public domain JAMA package.
    /// http://math.nist.gov/javanumerics/jama/
    /// </summary>
    public class EVD
    {
        /// <summary>
        /// Arrays for internal storage of eigenvalues.
        /// </summary>
        private readonly double[] _realEigenvalues;

        /// <summary>
        /// Arrays for internal storage of eigenvalues.
        /// </summary>
        private readonly double[] _imaginaryEigenvalues;

        /// <summary>
        /// Array for internal storage of nonsymmetric Hessenberg form.
        /// </summary>
        private readonly double[][] _hessenbergForm;

        /// <summary>
        /// Symmetry flag.
        /// </summary>
        private readonly bool _isSymmetric;

        /// <summary>
        /// Row and column dimension (square matrix).
        /// </summary>
        private readonly int _dimension;

        /// <summary>
        /// Working storage for nonsymmetric algorithm.
        /// </summary>
        private readonly double[] _ort;

        /// <summary>
        /// Array for internal storage of eigenvectors.
        /// </summary>
        private readonly double[][] _eigenvectors;

        /// <summary>
        /// Complex scalar division.
        /// </summary>
        private double _cdivi;

        /// <summary>
        /// Complex scalar division.
        /// </summary>
        private double _cdivr;

        //Constructor
        /// <summary>
        /// Check for symmetry, then construct the eigenvalue decomposition
        /// </summary>
        /// <param name="matrix">Square matrix</param>
        public EVD(Matrix matrix)
        {
            double[][] matrixData = matrix.Data;
            _dimension = matrix.NumOfCols;
            if(_dimension != matrix.NumOfRows)
            {
                throw new ArgumentException("Matrix is not square.", "matrix");
            }

            _eigenvectors = new double[_dimension][];
            Parallel.For(0, _dimension, row =>
            {
                _eigenvectors[row] = new double[_dimension];
                _eigenvectors[row].Populate(0);
            });

            _realEigenvalues = new double[_dimension];
            _imaginaryEigenvalues = new double[_dimension];

            _isSymmetric = true;
            for (int j = 0; (j < _dimension) & _isSymmetric; j++)
            {
                for (int i = 0; (i < _dimension) & _isSymmetric; i++)
                {
                    _isSymmetric = (matrixData[i][j] == matrixData[j][i]);
                }
            }

            if (_isSymmetric)
            {
                Parallel.For(0, _dimension, i =>
               {
                   for (int j = 0; j < _dimension; j++)
                   {
                       _eigenvectors[i][j] = matrixData[i][j];
                   }
               });

                // Tridiagonalize.
                Tred2();

                // Diagonalize.
                Tql2();
            }
            else
            {
                _hessenbergForm = new double[_dimension][];
                Parallel.For(0, _dimension, row =>
                {
                    _hessenbergForm[row] = new double[_dimension];
                    _hessenbergForm[row].Populate(0);
                });
                _ort = new double[_dimension];

                Parallel.For(0, _dimension, i =>
                {
                    for (int j = 0; j < _dimension; j++)
                    {
                        _hessenbergForm[i][j] = matrixData[i][j];
                    }
                });

                // Reduce to Hessenberg form.
                Orthes();

                // Reduce Hessenberg to real Schur form.
                Hqr2();
            }
        }

        //Properties
        /// <summary>
        /// Return the eigenvector matrix.
        /// </summary>
        public Matrix V
        {
            get { return new Matrix(_eigenvectors); }
        }

        /// <summary>
        /// Return the real parts of the eigenvalues.
        /// </summary>
        public double[] RealEigenvalues
        {
            get { return _realEigenvalues; }
        }

        /// <summary>
        /// Return the max real eigenvalue.
        /// </summary>
        public double MaxAbsRealEigenvalue
        {
            get
            {
                double max = Math.Abs(_realEigenvalues[0]);
                for(int i = 1; i < _realEigenvalues.Length; i++)
                {
                    max = Math.Max(max, Math.Abs(_realEigenvalues[i]));
                }
                return max;
            }
        }

        /// <summary>
        /// Return the imaginary parts of the eigenvalues.
        /// </summary>
        public double[] ImaginaryEigenvalues
        {
            get { return _imaginaryEigenvalues; }
        }

        /// <summary>
        /// Return the block diagonal eigenvalue matrix
        /// </summary>
        public Matrix D
        {
            get
            {
                var result = new Matrix(_dimension, _dimension);
                double[][] resultMatrix = result.Data;
                for (int i = 0; i < _dimension; i++)
                {
                    for (int j = 0; j < _dimension; j++)
                    {
                        resultMatrix[i][j] = 0.0;
                    }
                    resultMatrix[i][i] = _realEigenvalues[i];
                    if (_imaginaryEigenvalues[i] > 0)
                    {
                        resultMatrix[i][i + 1] = _imaginaryEigenvalues[i];
                    }
                    else if (_imaginaryEigenvalues[i] < 0)
                    {
                        resultMatrix[i][i - 1] = _imaginaryEigenvalues[i];
                    }
                }
                return result;
            }
        }

        //Methods
        /// <summary>
        /// Symmetric Householder reduction to tridiagonal form.
        /// </summary>
        private void Tred2()
        {
            // This is derived from the Algol procedures tred2 by
            // Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
            // Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
            // Fortran subroutine in EISPACK.

            for (int j = 0; j < _dimension; j++)
            {
                _realEigenvalues[j] = _eigenvectors[_dimension - 1][j];
            }

            // Householder reduction to tridiagonal form.

            for (int i = _dimension - 1; i > 0; i--)
            {
                // Scale to avoid under/overflow.

                double scale = 0.0;
                double h = 0.0;
                for (int k = 0; k < i; k++)
                {
                    scale = scale + Math.Abs(_realEigenvalues[k]);
                }
                if (scale == 0.0)
                {
                    _imaginaryEigenvalues[i] = _realEigenvalues[i - 1];
                    for (int j = 0; j < i; j++)
                    {
                        _realEigenvalues[j] = _eigenvectors[i - 1][j];
                        _eigenvectors[i][j] = 0.0;
                        _eigenvectors[j][i] = 0.0;
                    }
                }
                else
                {
                    // Generate Householder vector.

                    for (int k = 0; k < i; k++)
                    {
                        _realEigenvalues[k] /= scale;
                        h += _realEigenvalues[k] * _realEigenvalues[k];
                    }
                    double f = _realEigenvalues[i - 1];
                    double g = Math.Sqrt(h);
                    if (f > 0)
                    {
                        g = -g;
                    }
                    _imaginaryEigenvalues[i] = scale * g;
                    h = h - f * g;
                    _realEigenvalues[i - 1] = f - g;
                    for (int j = 0; j < i; j++)
                    {
                        _imaginaryEigenvalues[j] = 0.0;
                    }

                    // Apply similarity transformation to remaining columns.

                    for (int j = 0; j < i; j++)
                    {
                        f = _realEigenvalues[j];
                        _eigenvectors[j][i] = f;
                        g = _imaginaryEigenvalues[j] + _eigenvectors[j][j] * f;
                        for (int k = j + 1; k <= i - 1; k++)
                        {
                            g += _eigenvectors[k][j] * _realEigenvalues[k];
                            _imaginaryEigenvalues[k] += _eigenvectors[k][j] * f;
                        }
                        _imaginaryEigenvalues[j] = g;
                    }
                    f = 0.0;
                    for (int j = 0; j < i; j++)
                    {
                        _imaginaryEigenvalues[j] /= h;
                        f += _imaginaryEigenvalues[j] * _realEigenvalues[j];
                    }
                    double hh = f / (h + h);
                    for (int j = 0; j < i; j++)
                    {
                        _imaginaryEigenvalues[j] -= hh * _realEigenvalues[j];
                    }
                    for (int j = 0; j < i; j++)
                    {
                        f = _realEigenvalues[j];
                        g = _imaginaryEigenvalues[j];
                        for (int k = j; k <= i - 1; k++)
                        {
                            _eigenvectors[k][j] -= (f * _imaginaryEigenvalues[k] + g * _realEigenvalues[k]);
                        }
                        _realEigenvalues[j] = _eigenvectors[i - 1][j];
                        _eigenvectors[i][j] = 0.0;
                    }
                }
                _realEigenvalues[i] = h;
            }

            // Accumulate transformations.

            for (int i = 0; i < _dimension - 1; i++)
            {
                _eigenvectors[_dimension - 1][i] = _eigenvectors[i][i];
                _eigenvectors[i][i] = 1.0;
                double h = _realEigenvalues[i + 1];
                if (h != 0.0)
                {
                    for (int k = 0; k <= i; k++)
                    {
                        _realEigenvalues[k] = _eigenvectors[k][i + 1] / h;
                    }
                    for (int j = 0; j <= i; j++)
                    {
                        double g = 0.0;
                        for (int k = 0; k <= i; k++)
                        {
                            g += _eigenvectors[k][i + 1] * _eigenvectors[k][j];
                        }
                        for (int k = 0; k <= i; k++)
                        {
                            _eigenvectors[k][j] -= g * _realEigenvalues[k];
                        }
                    }
                }
                for (int k = 0; k <= i; k++)
                {
                    _eigenvectors[k][i + 1] = 0.0;
                }
            }
            for (int j = 0; j < _dimension; j++)
            {
                _realEigenvalues[j] = _eigenvectors[_dimension - 1][j];
                _eigenvectors[_dimension - 1][j] = 0.0;
            }
            _eigenvectors[_dimension - 1][_dimension - 1] = 1.0;
            _imaginaryEigenvalues[0] = 0.0;
        }

        /// <summary>
        /// Symmetric tridiagonal QL algorithm.
        /// </summary>
        private void Tql2()
        {
            // This is derived from the Algol procedures tql2, by
            // Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
            // Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
            // Fortran subroutine in EISPACK.

            for (int i = 1; i < _dimension; i++)
            {
                _imaginaryEigenvalues[i - 1] = _imaginaryEigenvalues[i];
            }
            _imaginaryEigenvalues[_dimension - 1] = 0.0;

            double f = 0.0;
            double tst1 = 0.0;
            double eps = Math.Pow(2.0, -52.0);
            for (int l = 0; l < _dimension; l++)
            {
                // Find small subdiagonal element

                tst1 = Math.Max(tst1, Math.Abs(_realEigenvalues[l]) + Math.Abs(_imaginaryEigenvalues[l]));
                int m = l;
                while (m < _dimension)
                {
                    if (Math.Abs(_imaginaryEigenvalues[m]) <= eps * tst1)
                    {
                        break;
                    }
                    m++;
                }

                // If m == l, d[l] is an eigenvalue,
                // otherwise, iterate.

                if (m > l)
                {
                    int iter = 0;
                    do
                    {
                        iter = iter + 1; // (Could check iteration count here.)

                        // Compute implicit shift

                        double g = _realEigenvalues[l];
                        double p = (_realEigenvalues[l + 1] - g) / (2.0 * _imaginaryEigenvalues[l]);
                        double r = Matrix.Hypotenuse(p, 1.0);
                        if (p < 0)
                        {
                            r = -r;
                        }
                        _realEigenvalues[l] = _imaginaryEigenvalues[l] / (p + r);
                        _realEigenvalues[l + 1] = _imaginaryEigenvalues[l] * (p + r);
                        double dl1 = _realEigenvalues[l + 1];
                        double h = g - _realEigenvalues[l];
                        for (int i = l + 2; i < _dimension; i++)
                        {
                            _realEigenvalues[i] -= h;
                        }
                        f = f + h;

                        // Implicit QL transformation.

                        p = _realEigenvalues[m];
                        double c = 1.0;
                        double c2 = c;
                        double c3 = c;
                        double el1 = _imaginaryEigenvalues[l + 1];
                        double s = 0.0;
                        double s2 = 0.0;
                        for (int i = m - 1; i >= l; i--)
                        {
                            c3 = c2;
                            c2 = c;
                            s2 = s;
                            g = c * _imaginaryEigenvalues[i];
                            h = c * p;
                            r = Matrix.Hypotenuse(p, _imaginaryEigenvalues[i]);
                            _imaginaryEigenvalues[i + 1] = s * r;
                            s = _imaginaryEigenvalues[i] / r;
                            c = p / r;
                            p = c * _realEigenvalues[i] - s * g;
                            _realEigenvalues[i + 1] = h + s * (c * g + s * _realEigenvalues[i]);

                            // Accumulate transformation.

                            for (int k = 0; k < _dimension; k++)
                            {
                                h = _eigenvectors[k][i + 1];
                                _eigenvectors[k][i + 1] = s * _eigenvectors[k][i] + c * h;
                                _eigenvectors[k][i] = c * _eigenvectors[k][i] - s * h;
                            }
                        }
                        p = -s * s2 * c3 * el1 * _imaginaryEigenvalues[l] / dl1;
                        _imaginaryEigenvalues[l] = s * p;
                        _realEigenvalues[l] = c * p;

                        // Check for convergence.
                    } while (Math.Abs(_imaginaryEigenvalues[l]) > eps * tst1);
                }
                _realEigenvalues[l] = _realEigenvalues[l] + f;
                _imaginaryEigenvalues[l] = 0.0;
            }

            // Sort eigenvalues and corresponding vectors.

            for (int i = 0; i < _dimension - 1; i++)
            {
                int k = i;
                double p = _realEigenvalues[i];
                for (int j = i + 1; j < _dimension; j++)
                {
                    if (_realEigenvalues[j] < p)
                    {
                        k = j;
                        p = _realEigenvalues[j];
                    }
                }
                if (k != i)
                {
                    _realEigenvalues[k] = _realEigenvalues[i];
                    _realEigenvalues[i] = p;
                    for (int j = 0; j < _dimension; j++)
                    {
                        p = _eigenvectors[j][i];
                        _eigenvectors[j][i] = _eigenvectors[j][k];
                        _eigenvectors[j][k] = p;
                    }
                }
            }
        }

        /// <summary>
        /// This is derived from the Algol procedures orthes and ortran, by Martin
        /// and Wilkinson, Handbook for Auto. Comp., Vol.ii-Linear Algebra, and the
        /// corresponding Fortran subroutines in EISPACK.
        /// </summary>
        private void Orthes()
        {
            int low = 0;
            int high = _dimension - 1;

            for (int m = low + 1; m <= high - 1; m++)
            {
                // Scale column.

                double scale = 0.0;
                for (int i = m; i <= high; i++)
                {
                    scale = scale + Math.Abs(_hessenbergForm[i][m - 1]);
                }
                if (scale != 0.0)
                {
                    // Compute Householder transformation.

                    double lh = 0.0;
                    for (int i = high; i >= m; i--)
                    {
                        _ort[i] = _hessenbergForm[i][m - 1] / scale;
                        lh += _ort[i] * _ort[i];
                    }
                    double g = Math.Sqrt(lh);
                    if (_ort[m] > 0)
                    {
                        g = -g;
                    }
                    lh = lh - _ort[m] * g;
                    _ort[m] = _ort[m] - g;

                    // Apply Householder similarity transformation
                    // H = (I-u*u'/h)*H*(I-u*u')/h)

                    for (int j = m; j < _dimension; j++)
                    {
                        double f = 0.0;
                        for (int i = high; i >= m; i--)
                        {
                            f += _ort[i] * _hessenbergForm[i][j];
                        }
                        f = f / lh;
                        for (int i = m; i <= high; i++)
                        {
                            _hessenbergForm[i][j] -= f * _ort[i];
                        }
                    }

                    for (int i = 0; i <= high; i++)
                    {
                        double f = 0.0;
                        for (int j = high; j >= m; j--)
                        {
                            f += _ort[j] * _hessenbergForm[i][j];
                        }
                        f = f / lh;
                        for (int j = m; j <= high; j++)
                        {
                            _hessenbergForm[i][j] -= f * _ort[j];
                        }
                    }
                    _ort[m] = scale * _ort[m];
                    _hessenbergForm[m][m - 1] = scale * g;
                }
            }

            // Accumulate transformations (Algol's ortran).

            Parallel.For(0, _dimension, i =>
            {
                for (int j = 0; j < _dimension; j++)
                {
                    _eigenvectors[i][j] = (i == j ? 1.0 : 0.0);
                }
            });

            for (int m = high - 1; m >= low + 1; m--)
            {
                if (_hessenbergForm[m][m - 1] != 0.0)
                {
                    for (int i = m + 1; i <= high; i++)
                    {
                        _ort[i] = _hessenbergForm[i][m - 1];
                    }
                    for (int j = m; j <= high; j++)
                    {
                        double g = 0.0;
                        for (int i = m; i <= high; i++)
                        {
                            g += _ort[i] * _eigenvectors[i][j];
                        }
                        // Double division avoids possible underflow
                        g = (g / _ort[m]) / _hessenbergForm[m][m - 1];
                        for (int i = m; i <= high; i++)
                        {
                            _eigenvectors[i][j] += g * _ort[i];
                        }
                    }
                }
            }
        }

        private void cdiv(double xr, double xi, double yr, double yi)
        {
            double r, d;
            if (Math.Abs(yr) > Math.Abs(yi))
            {
                r = yi / yr;
                d = yr + r * yi;
                _cdivr = (xr + r * xi) / d;
                _cdivi = (xi - r * xr) / d;
            }
            else
            {
                r = yr / yi;
                d = yi + r * yr;
                _cdivr = (r * xr + xi) / d;
                _cdivi = (r * xi - xr) / d;
            }
        }

        /// <summary>
        /// This is derived from the Algol procedure hqr2, by Martin and Wilkinson,
        /// Handbook for Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
        /// Fortran subroutine in EISPACK.
        /// </summary>
        private void Hqr2()
        {
            // Initialize
            int nn = _dimension;
            int n = nn - 1;
            int low = 0;
            int high = nn - 1;
            double eps = Math.Pow(2.0, -52.0);
            double exshift = 0.0;
            double p = 0, q = 0, r = 0, s = 0, z = 0, t, w, x, y;

            // Store roots isolated by balanc and compute matrix norm

            double norm = 0.0;
            for (int i = 0; i < nn; i++)
            {
                if (i < low | i > high)
                {
                    _realEigenvalues[i] = _hessenbergForm[i][i];
                    _imaginaryEigenvalues[i] = 0.0;
                }
                for (int j = Math.Max(i - 1, 0); j < nn; j++)
                {
                    norm = norm + Math.Abs(_hessenbergForm[i][j]);
                }
            }

            // Outer loop over eigenvalue index

            int iter = 0;
            while (n >= low)
            {
                // Look for single small sub-diagonal element

                int l = n;
                while (l > low)
                {
                    s = Math.Abs(_hessenbergForm[l - 1][l - 1]) + Math.Abs(_hessenbergForm[l][l]);
                    if (s == 0.0)
                    {
                        s = norm;
                    }
                    if (Math.Abs(_hessenbergForm[l][l - 1]) < eps * s)
                    {
                        break;
                    }
                    l--;
                }

                // Check for convergence
                // One root found

                if (l == n)
                {
                    _hessenbergForm[n][n] = _hessenbergForm[n][n] + exshift;
                    _realEigenvalues[n] = _hessenbergForm[n][n];
                    _imaginaryEigenvalues[n] = 0.0;
                    n--;
                    iter = 0;

                    // Two roots found
                }
                else if (l == n - 1)
                {
                    w = _hessenbergForm[n][n - 1] * _hessenbergForm[n - 1][n];
                    p = (_hessenbergForm[n - 1][n - 1] - _hessenbergForm[n][n]) / 2.0;
                    q = p * p + w;
                    z = Math.Sqrt(Math.Abs(q));
                    _hessenbergForm[n][n] = _hessenbergForm[n][n] + exshift;
                    _hessenbergForm[n - 1][n - 1] = _hessenbergForm[n - 1][n - 1] + exshift;
                    x = _hessenbergForm[n][n];

                    // Real pair

                    if (q >= 0)
                    {
                        if (p >= 0)
                        {
                            z = p + z;
                        }
                        else
                        {
                            z = p - z;
                        }
                        _realEigenvalues[n - 1] = x + z;
                        _realEigenvalues[n] = _realEigenvalues[n - 1];
                        if (z != 0.0)
                        {
                            _realEigenvalues[n] = x - w / z;
                        }
                        _imaginaryEigenvalues[n - 1] = 0.0;
                        _imaginaryEigenvalues[n] = 0.0;
                        x = _hessenbergForm[n][n - 1];
                        s = Math.Abs(x) + Math.Abs(z);
                        p = x / s;
                        q = z / s;
                        r = Math.Sqrt(p * p + q * q);
                        p = p / r;
                        q = q / r;

                        // Row modification

                        for (int j = n - 1; j < nn; j++)
                        {
                            z = _hessenbergForm[n - 1][j];
                            _hessenbergForm[n - 1][j] = q * z + p * _hessenbergForm[n][j];
                            _hessenbergForm[n][j] = q * _hessenbergForm[n][j] - p * z;
                        }

                        // Column modification

                        for (int i = 0; i <= n; i++)
                        {
                            z = _hessenbergForm[i][n - 1];
                            _hessenbergForm[i][n - 1] = q * z + p * _hessenbergForm[i][n];
                            _hessenbergForm[i][n] = q * _hessenbergForm[i][n] - p * z;
                        }

                        // Accumulate transformations

                        for (int i = low; i <= high; i++)
                        {
                            z = _eigenvectors[i][n - 1];
                            _eigenvectors[i][n - 1] = q * z + p * _eigenvectors[i][n];
                            _eigenvectors[i][n] = q * _eigenvectors[i][n] - p * z;
                        }

                        // Complex pair
                    }
                    else
                    {
                        _realEigenvalues[n - 1] = x + p;
                        _realEigenvalues[n] = x + p;
                        _imaginaryEigenvalues[n - 1] = z;
                        _imaginaryEigenvalues[n] = -z;
                    }
                    n = n - 2;
                    iter = 0;

                    // No convergence yet
                }
                else
                {
                    // Form shift

                    x = _hessenbergForm[n][n];
                    y = 0.0;
                    w = 0.0;
                    if (l < n)
                    {
                        y = _hessenbergForm[n - 1][n - 1];
                        w = _hessenbergForm[n][n - 1] * _hessenbergForm[n - 1][n];
                    }

                    // Wilkinson's original ad hoc shift

                    if (iter == 10)
                    {
                        exshift += x;
                        for (int i = low; i <= n; i++)
                        {
                            _hessenbergForm[i][i] -= x;
                        }
                        s = Math.Abs(_hessenbergForm[n][n - 1]) + Math.Abs(_hessenbergForm[n - 1][n - 2]);
                        x = y = 0.75 * s;
                        w = -0.4375 * s * s;
                    }

                    // MATLAB's new ad hoc shift

                    if (iter == 30)
                    {
                        s = (y - x) / 2.0;
                        s = s * s + w;
                        if (s > 0)
                        {
                            s = Math.Sqrt(s);
                            if (y < x)
                            {
                                s = -s;
                            }
                            s = x - w / ((y - x) / 2.0 + s);
                            for (int i = low; i <= n; i++)
                            {
                                _hessenbergForm[i][i] -= s;
                            }
                            exshift += s;
                            x = y = w = 0.964;
                        }
                    }

                    iter = iter + 1; // (Could check iteration count here.)

                    // Look for two consecutive small sub-diagonal elements

                    int m = n - 2;
                    while (m >= l)
                    {
                        z = _hessenbergForm[m][m];
                        r = x - z;
                        s = y - z;
                        p = (r * s - w) / _hessenbergForm[m + 1][m] + _hessenbergForm[m][m + 1];
                        q = _hessenbergForm[m + 1][m + 1] - z - r - s;
                        r = _hessenbergForm[m + 2][m + 1];
                        s = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                        p = p / s;
                        q = q / s;
                        r = r / s;
                        if (m == l)
                        {
                            break;
                        }
                        if (Math.Abs(_hessenbergForm[m][m - 1]) * (Math.Abs(q) + Math.Abs(r)) < eps
                            * (Math.Abs(p) * (Math.Abs(_hessenbergForm[m - 1][m - 1])
                                           + Math.Abs(z) + Math.Abs(_hessenbergForm[m + 1][m + 1]))))
                        {
                            break;
                        }
                        m--;
                    }

                    for (int i = m + 2; i <= n; i++)
                    {
                        _hessenbergForm[i][i - 2] = 0.0;
                        if (i > m + 2)
                        {
                            _hessenbergForm[i][i - 3] = 0.0;
                        }
                    }

                    // Double QR step involving rows l:n and columns m:n

                    for (int k = m; k <= n - 1; k++)
                    {
                        bool notlast = (k != n - 1);
                        if (k != m)
                        {
                            p = _hessenbergForm[k][k - 1];
                            q = _hessenbergForm[k + 1][k - 1];
                            r = (notlast ? _hessenbergForm[k + 2][k - 1] : 0.0);
                            x = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                            if (x != 0.0)
                            {
                                p = p / x;
                                q = q / x;
                                r = r / x;
                            }
                        }
                        if (x == 0.0)
                        {
                            break;
                        }
                        s = Math.Sqrt(p * p + q * q + r * r);
                        if (p < 0)
                        {
                            s = -s;
                        }
                        if (s != 0)
                        {
                            if (k != m)
                            {
                                _hessenbergForm[k][k - 1] = -s * x;
                            }
                            else if (l != m)
                            {
                                _hessenbergForm[k][k - 1] = -_hessenbergForm[k][k - 1];
                            }
                            p = p + s;
                            x = p / s;
                            y = q / s;
                            z = r / s;
                            q = q / p;
                            r = r / p;

                            // Row modification

                            for (int j = k; j < nn; j++)
                            {
                                p = _hessenbergForm[k][j] + q * _hessenbergForm[k + 1][j];
                                if (notlast)
                                {
                                    p = p + r * _hessenbergForm[k + 2][j];
                                    _hessenbergForm[k + 2][j] = _hessenbergForm[k + 2][j] - p * z;
                                }
                                _hessenbergForm[k][j] = _hessenbergForm[k][j] - p * x;
                                _hessenbergForm[k + 1][j] = _hessenbergForm[k + 1][j] - p * y;
                            }

                            // Column modification

                            for (int i = 0; i <= Math.Min(n, k + 3); i++)
                            {
                                p = x * _hessenbergForm[i][k] + y * _hessenbergForm[i][k + 1];
                                if (notlast)
                                {
                                    p = p + z * _hessenbergForm[i][k + 2];
                                    _hessenbergForm[i][k + 2] = _hessenbergForm[i][k + 2] - p * r;
                                }
                                _hessenbergForm[i][k] = _hessenbergForm[i][k] - p;
                                _hessenbergForm[i][k + 1] = _hessenbergForm[i][k + 1] - p * q;
                            }

                            // Accumulate transformations

                            for (int i = low; i <= high; i++)
                            {
                                p = x * _eigenvectors[i][k] + y * _eigenvectors[i][k + 1];
                                if (notlast)
                                {
                                    p = p + z * _eigenvectors[i][k + 2];
                                    _eigenvectors[i][k + 2] = _eigenvectors[i][k + 2] - p * r;
                                }
                                _eigenvectors[i][k] = _eigenvectors[i][k] - p;
                                _eigenvectors[i][k + 1] = _eigenvectors[i][k + 1] - p * q;
                            }
                        } // (s != 0)
                    } // k loop
                } // check convergence
            } // while (n >= low)

            // Backsubstitute to find vectors of upper triangular form

            if (norm == 0.0)
            {
                return;
            }

            for (n = nn - 1; n >= 0; n--)
            {
                p = _realEigenvalues[n];
                q = _imaginaryEigenvalues[n];

                // Real vector

                if (q == 0)
                {
                    int l = n;
                    _hessenbergForm[n][n] = 1.0;
                    for (int i = n - 1; i >= 0; i--)
                    {
                        w = _hessenbergForm[i][i] - p;
                        r = 0.0;
                        for (int j = l; j <= n; j++)
                        {
                            r = r + _hessenbergForm[i][j] * _hessenbergForm[j][n];
                        }
                        if (_imaginaryEigenvalues[i] < 0.0)
                        {
                            z = w;
                            s = r;
                        }
                        else
                        {
                            l = i;
                            if (_imaginaryEigenvalues[i] == 0.0)
                            {
                                if (w != 0.0)
                                {
                                    _hessenbergForm[i][n] = -r / w;
                                }
                                else
                                {
                                    _hessenbergForm[i][n] = -r / (eps * norm);
                                }

                                // Solve real equations
                            }
                            else
                            {
                                x = _hessenbergForm[i][i + 1];
                                y = _hessenbergForm[i + 1][i];
                                q = (_realEigenvalues[i] - p) * (_realEigenvalues[i] - p) + _imaginaryEigenvalues[i] * _imaginaryEigenvalues[i];
                                t = (x * s - z * r) / q;
                                _hessenbergForm[i][n] = t;
                                if (Math.Abs(x) > Math.Abs(z))
                                {
                                    _hessenbergForm[i + 1][n] = (-r - w * t) / x;
                                }
                                else
                                {
                                    _hessenbergForm[i + 1][n] = (-s - y * t) / z;
                                }
                            }

                            // Overflow control

                            t = Math.Abs(_hessenbergForm[i][n]);
                            if ((eps * t) * t > 1)
                            {
                                for (int j = i; j <= n; j++)
                                {
                                    _hessenbergForm[j][n] = _hessenbergForm[j][n] / t;
                                }
                            }
                        }
                    }

                    // Complex vector
                }
                else if (q < 0)
                {
                    int l = n - 1;

                    // Last vector component imaginary so matrix is triangular

                    if (Math.Abs(_hessenbergForm[n][n - 1]) > Math.Abs(_hessenbergForm[n - 1][n]))
                    {
                        _hessenbergForm[n - 1][n - 1] = q / _hessenbergForm[n][n - 1];
                        _hessenbergForm[n - 1][n] = -(_hessenbergForm[n][n] - p) / _hessenbergForm[n][n - 1];
                    }
                    else
                    {
                        cdiv(0.0, -_hessenbergForm[n - 1][n], _hessenbergForm[n - 1][n - 1] - p, q);
                        _hessenbergForm[n - 1][n - 1] = _cdivr;
                        _hessenbergForm[n - 1][n] = _cdivi;
                    }
                    _hessenbergForm[n][n - 1] = 0.0;
                    _hessenbergForm[n][n] = 1.0;
                    for (int i = n - 2; i >= 0; i--)
                    {
                        double ra, sa, vr, vi;
                        ra = 0.0;
                        sa = 0.0;
                        for (int j = l; j <= n; j++)
                        {
                            ra = ra + _hessenbergForm[i][j] * _hessenbergForm[j][n - 1];
                            sa = sa + _hessenbergForm[i][j] * _hessenbergForm[j][n];
                        }
                        w = _hessenbergForm[i][i] - p;

                        if (_imaginaryEigenvalues[i] < 0.0)
                        {
                            z = w;
                            r = ra;
                            s = sa;
                        }
                        else
                        {
                            l = i;
                            if (_imaginaryEigenvalues[i] == 0)
                            {
                                cdiv(-ra, -sa, w, q);
                                _hessenbergForm[i][n - 1] = _cdivr;
                                _hessenbergForm[i][n] = _cdivi;
                            }
                            else
                            {
                                // Solve complex equations

                                x = _hessenbergForm[i][i + 1];
                                y = _hessenbergForm[i + 1][i];
                                vr = (_realEigenvalues[i] - p) * (_realEigenvalues[i] - p) + _imaginaryEigenvalues[i] * _imaginaryEigenvalues[i] - q * q;
                                vi = (_realEigenvalues[i] - p) * 2.0 * q;
                                if (vr == 0.0 & vi == 0.0)
                                {
                                    vr = eps
                                         * norm
                                         * (Math.Abs(w) + Math.Abs(q)
                                           + Math.Abs(x) + Math.Abs(y) + Math
                                                                             .Abs(z));
                                }
                                cdiv(x * r - z * ra + q * sa, x * s - z * sa - q
                                                        * ra, vr, vi);
                                _hessenbergForm[i][n - 1] = _cdivr;
                                _hessenbergForm[i][n] = _cdivi;
                                if (Math.Abs(x) > (Math.Abs(z) + Math.Abs(q)))
                                {
                                    _hessenbergForm[i + 1][n - 1] = (-ra - w * _hessenbergForm[i][n - 1] + q
                                                       * _hessenbergForm[i][n])
                                                      / x;
                                    _hessenbergForm[i + 1][n] = (-sa - w * _hessenbergForm[i][n] - q
                                                   * _hessenbergForm[i][n - 1])
                                                  / x;
                                }
                                else
                                {
                                    cdiv(-r - y * _hessenbergForm[i][n - 1], -s - y * _hessenbergForm[i][n], z,
                                         q);
                                    _hessenbergForm[i + 1][n - 1] = _cdivr;
                                    _hessenbergForm[i + 1][n] = _cdivi;
                                }
                            }

                            // Overflow control

                            t = Math.Max(Math.Abs(_hessenbergForm[i][n - 1]), Math.Abs(_hessenbergForm[i][n]));
                            if ((eps * t) * t > 1)
                            {
                                for (int j = i; j <= n; j++)
                                {
                                    _hessenbergForm[j][n - 1] = _hessenbergForm[j][n - 1] / t;
                                    _hessenbergForm[j][n] = _hessenbergForm[j][n] / t;
                                }
                            }
                        }
                    }
                }
            }

            // Vectors of isolated roots

            for (int i = 0; i < nn; i++)
            {
                if (i < low | i > high)
                {
                    for (int j = i; j < nn; j++)
                    {
                        _eigenvectors[i][j] = _hessenbergForm[i][j];
                    }
                }
            }

            // Back transformation to get eigenvectors of original matrix

            for (int j = nn - 1; j >= low; j--)
            {
                for (int i = low; i <= high; i++)
                {
                    z = 0.0;
                    for (int k = low; k <= Math.Min(j, high); k++)
                    {
                        z = z + _eigenvectors[i][k] * _hessenbergForm[k][j];
                    }
                    _eigenvectors[i][j] = z;
                }
            }
        }

    }//EVD
}//Namespace

