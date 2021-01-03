using RCNet.Extensions;
using System;

namespace RCNet.MathTools.MatrixMath
{

    /// <summary>
    /// Implements the Singular Value decomposition of a matrix.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is based on a class from the public domain JAMA package.
    /// </para>
    /// <para>
    /// http://math.nist.gov/javanumerics/jama/
    /// </para>
    /// </remarks>
    public class SVD
    {
        //Attributes
        /// <summary>
        /// Number of rows
        /// </summary>
        private readonly int _numOfRows;

        /// <summary>
        /// Number of columns
        /// </summary>
        private readonly int _numOfCols;

        /// <summary>
        /// Computed singular values.
        /// </summary>
        private readonly double[] _singularValues;

        /// <summary>
        /// The U matrix (left singular vectors) data
        /// </summary>
        private readonly double[][] _uMatrixData;

        /// <summary>
        /// The V matrix (right singular vectors) data
        /// </summary>
        private readonly double[][] _vMatrixData;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <param name="prepareUMatrix">Specifies whether to generate left singular vectors matrix (U).</param>
        /// <param name="prepareVMatrix">Specifies whether to generate right singular vectors matrix (V).</param>
        public SVD(Matrix matrix, bool prepareUMatrix = true, bool prepareVMatrix = true)
        {
            //Initialization
            //Copy source matrix data
            double[][] A = matrix.GetDataClone();
            _numOfRows = matrix.NumOfRows;
            _numOfCols = matrix.NumOfCols;

            //Singular values
            _singularValues = new double[Math.Min(_numOfRows + 1, _numOfCols)];

            //U Matrix
            int numOfUMatrixCols = Math.Min(_numOfRows, _numOfCols);
            _uMatrixData = null;
            if (prepareUMatrix)
            {
                _uMatrixData = new double[_numOfRows][];
                for (int i = 0; i < _numOfRows; i++)
                {
                    _uMatrixData[i] = new double[numOfUMatrixCols];
                    _uMatrixData[i].Populate(0);
                }
            }

            //V Matrix
            _vMatrixData = null;
            if (prepareVMatrix)
            {
                _vMatrixData = new double[_numOfCols][];
                for (int i = 0; i < _numOfCols; i++)
                {
                    _vMatrixData[i] = new double[_numOfCols];
                    _vMatrixData[i].Populate(0);
                }
            }

            //Reduce A to bidiagonal form, storing the diagonal elements
            //in singular values array and the super-diagonal elements in e array
            double[] e = new double[_numOfCols];
            double[] work = new double[_numOfRows];
            int nct = Math.Min(_numOfRows - 1, _numOfCols);
            int nrt = Math.Max(0, Math.Min(_numOfCols - 2, _numOfRows));
            for (int k = 0; k < Math.Max(nct, nrt); k++)
            {
                if (k < nct)
                {
                    // Compute the transformation for the k-th column and
                    // place the k-th diagonal in s[k].
                    // Compute 2-norm of k-th column without under/overflow.
                    _singularValues[k] = 0;
                    for (int i = k; i < _numOfRows; i++)
                    {
                        _singularValues[k] = Matrix.Hypotenuse(_singularValues[k], A[i][k]);
                    }
                    if (_singularValues[k] != 0.0)
                    {
                        if (A[k][k] < 0.0)
                        {
                            _singularValues[k] = -_singularValues[k];
                        }
                        for (int i = k; i < _numOfRows; i++)
                        {
                            A[i][k] /= _singularValues[k];
                        }
                        A[k][k] += 1.0;
                    }
                    _singularValues[k] = -_singularValues[k];
                }
                for (int j = k + 1; j < _numOfCols; j++)
                {
                    if ((k < nct) & (_singularValues[k] != 0.0))
                    {
                        //Apply the transformation
                        double t = 0;
                        for (int i = k; i < _numOfRows; i++)
                        {
                            t += A[i][k] * A[i][j];
                        }
                        t = -t / A[k][k];
                        for (int i = k; i < _numOfRows; i++)
                        {
                            A[i][j] += t * A[i][k];
                        }
                    }
                    //Place the k-th row of A into e for the
                    //subsequent calculation of the row transformation.
                    e[j] = A[k][j];
                }
                if (prepareUMatrix & (k < nct))
                {
                    //Place the transformation in U for subsequent back
                    //multiplication.
                    for (int i = k; i < _numOfRows; i++)
                    {
                        _uMatrixData[i][k] = A[i][k];
                    }
                }
                if (k < nrt)
                {
                    //Compute the k-th row transformation and place the
                    //k-th super-diagonal in e[k].
                    //Compute 2-norm without under/overflow.
                    e[k] = 0;
                    for (int i = k + 1; i < _numOfCols; i++)
                    {
                        e[k] = Matrix.Hypotenuse(e[k], e[i]);
                    }
                    if (e[k] != 0.0)
                    {
                        if (e[k + 1] < 0.0)
                        {
                            e[k] = -e[k];
                        }
                        for (int i = k + 1; i < _numOfCols; i++)
                        {
                            e[i] /= e[k];
                        }
                        e[k + 1] += 1.0;
                    }
                    e[k] = -e[k];
                    if ((k + 1 < _numOfRows) & (e[k] != 0.0))
                    {
                        //Apply the transformation.
                        for (int i = k + 1; i < _numOfRows; i++)
                        {
                            work[i] = 0.0;
                        }
                        for (int j = k + 1; j < _numOfCols; j++)
                        {
                            for (int i = k + 1; i < _numOfRows; i++)
                            {
                                work[i] += e[j] * A[i][j];
                            }
                        }
                        for (int j = k + 1; j < _numOfCols; j++)
                        {
                            double t = -e[j] / e[k + 1];
                            for (int i = k + 1; i < _numOfRows; i++)
                            {
                                A[i][j] += t * work[i];
                            }
                        }
                    }
                    if (prepareVMatrix)
                    {
                        //Place the transformation in V for subsequent
                        //back multiplication.
                        for (int i = k + 1; i < _numOfCols; i++)
                        {
                            _vMatrixData[i][k] = e[i];
                        }
                    }
                }
            }

            // Set up the final bidiagonal matrix or order p.
            int p = Math.Min(_numOfCols, _numOfRows + 1);
            if (nct < _numOfCols)
            {
                _singularValues[nct] = A[nct][nct];
            }
            if (_numOfRows < p)
            {
                _singularValues[p - 1] = 0.0;
            }
            if (nrt + 1 < p)
            {
                e[nrt] = A[nrt][p - 1];
            }
            e[p - 1] = 0.0;

            //If required, generate U matrix
            if (prepareUMatrix)
            {
                for (int j = nct; j < numOfUMatrixCols; j++)
                {
                    for (int i = 0; i < _numOfRows; i++)
                    {
                        _uMatrixData[i][j] = 0.0;
                    }
                    _uMatrixData[j][j] = 1.0;
                }
                for (int k = nct - 1; k >= 0; k--)
                {
                    if (_singularValues[k] != 0.0)
                    {
                        for (int j = k + 1; j < numOfUMatrixCols; j++)
                        {
                            double t = 0;
                            for (int i = k; i < _numOfRows; i++)
                            {
                                t += _uMatrixData[i][k] * _uMatrixData[i][j];
                            }
                            t = -t / _uMatrixData[k][k];
                            for (int i = k; i < _numOfRows; i++)
                            {
                                _uMatrixData[i][j] += t * _uMatrixData[i][k];
                            }
                        }
                        for (int i = k; i < _numOfRows; i++)
                        {
                            _uMatrixData[i][k] = -_uMatrixData[i][k];
                        }
                        _uMatrixData[k][k] = 1.0 + _uMatrixData[k][k];
                        for (int i = 0; i < k - 1; i++)
                        {
                            _uMatrixData[i][k] = 0.0;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _numOfRows; i++)
                        {
                            _uMatrixData[i][k] = 0.0;
                        }
                        _uMatrixData[k][k] = 1.0;
                    }
                }
            }

            //If required, generate V matrix
            if (prepareVMatrix)
            {
                for (int k = _numOfCols - 1; k >= 0; k--)
                {
                    if ((k < nrt) & (e[k] != 0.0))
                    {
                        for (int j = k + 1; j < numOfUMatrixCols; j++)
                        {
                            double t = 0;
                            for (int i = k + 1; i < _numOfCols; i++)
                            {
                                t += _vMatrixData[i][k] * _vMatrixData[i][j];
                            }
                            t = -t / _vMatrixData[k + 1][k];
                            for (int i = k + 1; i < _numOfCols; i++)
                            {
                                _vMatrixData[i][j] += t * _vMatrixData[i][k];
                            }
                        }
                    }
                    for (int i = 0; i < _numOfCols; i++)
                    {
                        _vMatrixData[i][k] = 0.0;
                    }
                    _vMatrixData[k][k] = 1.0;
                }
            }

            //Main iteration loop for the singular values
            int pp = p - 1;
            int iter = 0;
            double eps = Math.Pow(2.0, -52.0);
            double tiny = Math.Pow(2.0, -966.0);
            while (p > 0)
            {
                int k = 0, kase = 0;

                // Here is where a test for too many iterations would go.

                // This section of the program inspects for
                // negligible elements in the s ingular values and e arrays. On
                // completion the variables kase and k are set as follows.

                // kase = 1 if s(p) and e[k-1] are negligible and k<p
                // kase = 2 if s(k) is negligible and k<p
                // kase = 3 if e[k-1] is negligible, k<p, and
                // s(k), ..., s(p) are not negligible (qr step).
                // kase = 4 if e(p-1) is negligible (convergence).

                for (k = p - 2; k >= -1; k--)
                {
                    if (k == -1)
                    {
                        break;
                    }
                    if (Math.Abs(e[k]) <= tiny + eps * (Math.Abs(_singularValues[k]) + Math.Abs(_singularValues[k + 1])))
                    {
                        e[k] = 0.0;
                        break;
                    }
                }
                if (k == p - 2)
                {
                    kase = 4;
                }
                else
                {
                    int ks;
                    for (ks = p - 1; ks >= k; ks--)
                    {
                        if (ks == k)
                        {
                            break;
                        }
                        double t = (ks != p ? Math.Abs(e[ks]) : 0.0) + (ks != k + 1 ? Math.Abs(e[ks - 1]) : 0.0);
                        if (Math.Abs(_singularValues[ks]) <= tiny + eps * t)
                        {
                            _singularValues[ks] = 0.0;
                            break;
                        }
                    }
                    if (ks == k)
                    {
                        kase = 3;
                    }
                    else if (ks == p - 1)
                    {
                        kase = 1;
                    }
                    else
                    {
                        kase = 2;
                        k = ks;
                    }
                }
                k++;

                //Perform the task indicated by kase
                switch (kase)
                {
                    //Deflate negligible s(p)
                    case 1:
                        {
                            double f = e[p - 2];
                            e[p - 2] = 0.0;
                            for (int j = p - 2; j >= k; j--)
                            {
                                double t = Matrix.Hypotenuse(_singularValues[j], f);
                                double cs = _singularValues[j] / t;
                                double sn = f / t;
                                _singularValues[j] = t;
                                if (j != k)
                                {
                                    f = -sn * e[j - 1];
                                    e[j - 1] = cs * e[j - 1];
                                }
                                if (prepareVMatrix)
                                {
                                    for (int i = 0; i < _numOfCols; i++)
                                    {
                                        t = cs * _vMatrixData[i][j] + sn * _vMatrixData[i][p - 1];
                                        _vMatrixData[i][p - 1] = -sn * _vMatrixData[i][j] + cs * _vMatrixData[i][p - 1];
                                        _vMatrixData[i][j] = t;
                                    }
                                }
                            }
                        }
                        break;

                    //Split at negligible s(k)
                    case 2:
                        {
                            double f = e[k - 1];
                            e[k - 1] = 0.0;
                            for (int j = k; j < p; j++)
                            {
                                double t = Matrix.Hypotenuse(_singularValues[j], f);
                                double cs = _singularValues[j] / t;
                                double sn = f / t;
                                _singularValues[j] = t;
                                f = -sn * e[j];
                                e[j] = cs * e[j];
                                if (prepareUMatrix)
                                {
                                    for (int i = 0; i < _numOfRows; i++)
                                    {
                                        t = cs * _uMatrixData[i][j] + sn * _uMatrixData[i][k - 1];
                                        _uMatrixData[i][k - 1] = -sn * _uMatrixData[i][j] + cs * _uMatrixData[i][k - 1];
                                        _uMatrixData[i][j] = t;
                                    }
                                }
                            }
                        }
                        break;

                    //Perform one qr step
                    case 3:
                        {
                            // Calculate the shift.
                            double scale = Math.Max(Math.Max(Math.Max(Math.Max(Math.Abs(_singularValues[p - 1]), Math.Abs(_singularValues[p - 2])), Math.Abs(e[p - 2])), Math.Abs(_singularValues[k])), Math.Abs(e[k]));
                            double sp = _singularValues[p - 1] / scale;
                            double spm1 = _singularValues[p - 2] / scale;
                            double epm1 = e[p - 2] / scale;
                            double sk = _singularValues[k] / scale;
                            double ek = e[k] / scale;
                            double b = ((spm1 + sp) * (spm1 - sp) + epm1 * epm1) / 2.0;
                            double c = (sp * epm1) * (sp * epm1);
                            double shift = 0.0;
                            if ((b != 0.0) | (c != 0.0))
                            {
                                shift = Math.Sqrt(b * b + c);
                                if (b < 0.0)
                                {
                                    shift = -shift;
                                }
                                shift = c / (b + shift);
                            }
                            double f = (sk + sp) * (sk - sp) + shift;
                            double g = sk * ek;

                            //Chase zeros
                            for (int j = k; j < p - 1; j++)
                            {
                                double t = Matrix.Hypotenuse(f, g);
                                double cs = f / t;
                                double sn = g / t;
                                if (j != k)
                                {
                                    e[j - 1] = t;
                                }
                                f = cs * _singularValues[j] + sn * e[j];
                                e[j] = cs * e[j] - sn * _singularValues[j];
                                g = sn * _singularValues[j + 1];
                                _singularValues[j + 1] = cs * _singularValues[j + 1];
                                if (prepareVMatrix)
                                {
                                    for (int i = 0; i < _numOfCols; i++)
                                    {
                                        t = cs * _vMatrixData[i][j] + sn * _vMatrixData[i][j + 1];
                                        _vMatrixData[i][j + 1] = -sn * _vMatrixData[i][j] + cs * _vMatrixData[i][j + 1];
                                        _vMatrixData[i][j] = t;
                                    }
                                }
                                t = Matrix.Hypotenuse(f, g);
                                cs = f / t;
                                sn = g / t;
                                _singularValues[j] = t;
                                f = cs * e[j] + sn * _singularValues[j + 1];
                                _singularValues[j + 1] = -sn * e[j] + cs * _singularValues[j + 1];
                                g = sn * e[j + 1];
                                e[j + 1] = cs * e[j + 1];
                                if (prepareUMatrix && (j < _numOfRows - 1))
                                {
                                    for (int i = 0; i < _numOfRows; i++)
                                    {
                                        t = cs * _uMatrixData[i][j] + sn * _uMatrixData[i][j + 1];
                                        _uMatrixData[i][j + 1] = -sn * _uMatrixData[i][j] + cs * _uMatrixData[i][j + 1];
                                        _uMatrixData[i][j] = t;
                                    }
                                }
                            }
                            e[p - 2] = f;
                            ++iter;
                        }
                        break;

                    //Convergence.
                    case 4:
                        {
                            //Make the singular values positive
                            if (_singularValues[k] <= 0.0)
                            {
                                _singularValues[k] = (_singularValues[k] < 0.0 ? -_singularValues[k] : 0.0);
                                if (prepareVMatrix)
                                {
                                    for (int i = 0; i <= pp; i++)
                                    {
                                        _vMatrixData[i][k] = -_vMatrixData[i][k];
                                    }
                                }
                            }

                            //Order the singular values.
                            while (k < pp)
                            {
                                if (_singularValues[k] >= _singularValues[k + 1])
                                {
                                    break;
                                }
                                double t = _singularValues[k];
                                _singularValues[k] = _singularValues[k + 1];
                                _singularValues[k + 1] = t;
                                if (prepareVMatrix && (k < _numOfCols - 1))
                                {
                                    for (int i = 0; i < _numOfCols; i++)
                                    {
                                        t = _vMatrixData[i][k + 1];
                                        _vMatrixData[i][k + 1] = _vMatrixData[i][k];
                                        _vMatrixData[i][k] = t;
                                    }
                                }
                                if (prepareUMatrix && (k < _numOfRows - 1))
                                {
                                    for (int i = 0; i < _numOfRows; i++)
                                    {
                                        t = _uMatrixData[i][k + 1];
                                        _uMatrixData[i][k + 1] = _uMatrixData[i][k];
                                        _uMatrixData[i][k] = t;
                                    }
                                }
                                k++;
                            }
                            iter = 0;
                            p--;
                        }
                        break;
                }
            }
            return;
        }

        //Properties
        /// <summary>
        /// Gets the left singular vectors.
        /// </summary>
        public Matrix U
        {
            get
            {
                return _uMatrixData == null ? null : new Matrix(_uMatrixData);
            }
        }

        /// <summary>
        /// Gets the right singular vectors.
        /// </summary>
        public Matrix V
        {
            get
            {
                return _vMatrixData == null ? null : new Matrix(_vMatrixData);
            }
        }

        /// <summary>
        /// The singular values.
        /// </summary>
        public double[] SingularValues
        {
            get { return _singularValues; }
        }

        /// <summary>
        /// Gets the maximum singular value.
        /// </summary>
        public double MaxSingularValue
        {
            get { return _singularValues[0]; }
        }

        /// <summary>
        /// Gets the two norm condition number (max(singular value)/min(singular value)).
        /// </summary>
        public double Cond
        {
            get { return _singularValues[0] / _singularValues[Math.Min(_numOfRows, _numOfCols) - 1]; }
        }

        //Methods
        /// <summary>
        /// Creates the diagonal matrix of the singular values.
        /// </summary>
        public Matrix CreateDiagonalSVMatrix()
        {
            Matrix diagonalSVMatrix = new Matrix(_numOfCols, _numOfCols);
            for (int i = 0; i < _numOfCols; i++)
            {
                diagonalSVMatrix.Data[i][i] = _singularValues[i];
            }
            return diagonalSVMatrix;
        }

        /// <summary>
        /// Gets the matrix's effective numerical rank.
        /// </summary>
        public int GetRank()
        {
            double limit = Math.Max(_numOfRows, _numOfCols) * _singularValues[0] * Math.Pow(2.0, -52.0);
            int rank = 0;
            for (int i = 0; i < _singularValues.Length; i++)
            {
                if (_singularValues[i] > limit)
                {
                    ++rank;
                }
            }
            return rank;
        }

    }//SVD

}//Namespace

