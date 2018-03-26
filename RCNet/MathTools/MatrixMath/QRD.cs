using System;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.MathTools.MatrixMath
{
    /// <summary>
    /// QR Decomposition.
    /// This class is based on a class from the public domain JAMA package.
    /// http://math.nist.gov/javanumerics/jama/
    /// 
    /// Added parallel processing feature.
    /// </summary>
    public class QRD
    {
        //Attributes
        private int _numOfRows;
        private int _numOfCols;
        private double[][] _QRData;
        private double[] _RDiagData;

        //Constructor
        public QRD(Matrix source)
        {
            //Initialization
            _QRData = source.GetDataClone();
            _numOfRows = source.NumOfRows;
            _numOfCols = source.NumOfCols;
            _RDiagData = new double[_numOfCols];
            //Main loop
            for(int k = 0; k < _numOfCols; k++)
            {
                //Compute 2-norm of k-th column
                double norm = 0d;
                for (int i = k; i < _numOfRows; i++)
                {
                    norm = Hypotenuse(norm, _QRData[i][k]);
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
                throw new Exception("Matrix is rank deficient.");
            }
            return;
        }

        //Properties
        /// <summary>
        /// Returns the Householder vectors
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
        /// Returns the upper triangular factor
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
        /// Generates and returns the (economy-sized) orthogonal factor
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
                    if (_RDiagData[col] == 0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        //Methods
        //Static methods
        /// <summary>
        /// Calculates hypotenuse.
        /// https://en.wikipedia.org/wiki/Hypot
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        public static double Hypotenuse(double x, double y)
        {
            if (Math.Abs(x) > Math.Abs(y))
            {
                return Math.Abs(x) * Math.Sqrt(1d + (y / x).Power(2));
            }
            else if (y != 0)
            {
                return Math.Abs(y) * Math.Sqrt(1d + (x / y).Power(2));
            }
            else
            {
                return 0;
            }
        }

        //Instance methods
        /// <summary>
        /// Least squares solution of A*X = B
        /// </summary>
        /// <param name="B">A Matrix with as many rows as A and at least one column (desired values).</param>
        public Matrix Solve(Matrix B)
        {
            if (B.NumOfRows != _numOfRows)
            {
                throw new Exception("Different number of rows.");
            }

            // Copy right hand side
            int nx = B.NumOfCols;
            double[][] X = B.GetDataClone();

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
            return (new Matrix(X).GetSubMatrix(0, _numOfCols - 1, 0, nx - 1));
        }
    }//QRD

}//Namespace
