using System;
using System.Threading.Tasks;

namespace RCNet.MathTools.MatrixMath
{

    /// <summary>
    /// Implements the QR decomposition of a matrix.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is based on a class from the public domain JAMA package.
    /// RCNet project adds the parallel computation to improve the performance.
    /// </para>
    /// <para>
    /// http://math.nist.gov/javanumerics/jama/
    /// </para>
    /// </remarks>
    public class QRD
    {
        //Attributes
        private readonly int _numOfRows;
        private readonly int _numOfCols;
        private readonly double[][] _QRData;
        private readonly double[] _RDiagData;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        public QRD(Matrix matrix)
        {
            //Initialization
            _QRData = matrix.GetDataClone();
            _numOfRows = matrix.NumOfRows;
            _numOfCols = matrix.NumOfCols;
            _RDiagData = new double[_numOfCols];
            //Main loop
            for (int k = 0; k < _numOfCols; k++)
            {
                //Compute 2-norm of k-th column
                double norm = 0d;
                for (int i = k; i < _numOfRows; i++)
                {
                    norm = Matrix.Hypotenuse(norm, _QRData[i][k]);
                }//i
                if (norm != 0d)
                {
                    // Form k-th Householder vector.
                    if (_QRData[k][k] < 0d)
                    {
                        norm = -norm;
                    }
                    for (int i = k; i < _numOfRows; i++)
                    {
                        _QRData[i][k] /= norm;
                    }
                    _QRData[k][k] += 1d;
                    //Apply transformation to remaining columns.
                    Parallel.For(k + 1, _numOfCols, j =>
                    {
                        double s = 0.0;
                        for (int i = k; i < _numOfRows; i++)
                        {
                            s += _QRData[i][k] * _QRData[i][j];
                        }
                        s = -s / _QRData[k][k];
                        for (int i = k; i < _numOfRows; i++)
                        {
                            _QRData[i][j] += s * _QRData[i][k];
                        }
                    });
                }
                _RDiagData[k] = -norm;
            }//k
            if (!FullRank)
            {
                throw new ArgumentException($"Matrix is rank deficient.", "matrix");
            }
            return;
        }

        //Properties
        /// <summary>
        /// Returns the Householder vectors.
        /// </summary>
        public Matrix H
        {
            get
            {
                Matrix result = new Matrix(_numOfRows, _numOfCols);
                double[][] resultData = result.Data;
                Parallel.For(0, _numOfRows, row =>
                 {
                     for (int col = 0; col < _numOfCols; col++)
                     {
                         if (row >= col)
                         {
                             resultData[row][col] = _QRData[row][col];
                         }
                         else
                         {
                             resultData[row][col] = 0d;
                         }
                     }//col
                 });//row
                return result;
            }//get
        }//H

        /// <summary>
        /// Returns the upper triangular factor.
        /// </summary>
        public Matrix R
        {
            get
            {
                Matrix result = new Matrix(_numOfCols, _numOfCols);
                double[][] resultData = result.Data;
                Parallel.For(0, _numOfCols, i =>
                {
                    for (int j = 0; j < _numOfCols; j++)
                    {
                        if (i < j)
                        {
                            resultData[i][j] = _QRData[i][j];
                        }
                        else if (i == j)
                        {
                            resultData[i][j] = _RDiagData[i];
                        }
                        else
                        {
                            resultData[i][j] = 0d;
                        }
                    }//j
                });//i
                return result;
            }
        }//R

        /// <summary>
        /// Generates and returns the (economy-sized) orthogonal factor.
        /// </summary>
        public Matrix Q
        {
            get
            {
                Matrix result = new Matrix(_numOfRows, _numOfCols);
                double[][] resultData = result.Data;
                for (int k = _numOfCols - 1; k >= 0; k--)
                {
                    for (int row = 0; row < _numOfRows; row++)
                    {
                        resultData[row][k] = 0d;
                    }//row
                    resultData[k][k] = 1d;
                    for (int j = k; j < _numOfCols; j++)
                    {
                        if (_QRData[k][k] != 0d)
                        {
                            double s = 0d;
                            for (int i = k; i < _numOfRows; i++)
                            {
                                s += _QRData[i][k] * resultData[i][j];
                            }
                            s = -s / _QRData[k][k];
                            for (int i = k; i < _numOfRows; i++)
                            {
                                resultData[i][j] += s * _QRData[i][k];
                            }
                        }
                    }//j
                }//k
                return result;
            }
        }//Q


        /// <summary>
        /// Is full rank?
        /// </summary>
        public bool FullRank
        {
            get
            {
                for (int col = 0; col < _numOfCols; col++)
                {
                    /*
                    if (_RDiagData[col] == 0)
                    {
                        return false;
                    }
                    */
                    //Improved original zero condition to "close to zero" for the stability
                    if (Math.Abs(_RDiagData[col]) < 1E-20)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        //Methods
        //Instance methods
        /// <summary>
        /// Solves the least squares of A*X = B.
        /// </summary>
        /// <param name="matrixB">The matrix with as many rows as A and at least one column (desired values).</param>
        public Matrix Solve(Matrix matrixB)
        {
            //Check the number of rows in matrix B
            if (matrixB.NumOfRows != _numOfRows)
            {
                throw new ArgumentException($"Different number of rows in Matrix B.", "matrixB");
            }
            // Copy right hand side
            int nx = matrixB.NumOfCols;
            double[][] X = matrixB.GetDataClone();
            // Compute Y = transpose(Q)*B
            for (int k = 0; k < _numOfCols; k++)
            {
                for (int j = 0; j < nx; j++)
                {
                    double s = 0.0;
                    for (int i = k; i < _numOfRows; i++)
                    {
                        s += _QRData[i][k] * X[i][j];
                    }
                    s = -s / _QRData[k][k];
                    for (int i = k; i < _numOfRows; i++)
                    {
                        X[i][j] += s * _QRData[i][k];
                    }
                }
            }
            // Solve R*X = Y;
            for (int k = _numOfCols - 1; k >= 0; k--)
            {
                for (int j = 0; j < nx; j++)
                {
                    X[k][j] /= _RDiagData[k];
                }
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        X[i][j] -= X[k][j] * _QRData[i][k];
                    }
                }
            }
            return (new Matrix(X).CreateSubMatrix(0, _numOfCols - 1, 0, nx - 1));
        }

    }//QRD

}//Namespace
