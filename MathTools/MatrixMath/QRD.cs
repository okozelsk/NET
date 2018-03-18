using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKOSW.Extensions;

namespace OKOSW.MathTools.MatrixMath
{
    /// <summary>
    /// QR Decomposition.
    /// This class is based on a class from the public domain JAMA package.
    /// http://math.nist.gov/javanumerics/jama/
    /// </summary>
    public class QRD
    {
        //Constants

        //Attributes
        private double[][] m_QRData;
        private double[] m_RDiagData;
        private int m_rowsCount;
        private int m_colsCount;

        //Constructor
        public QRD(Matrix source)
        {
            //Initialization
            m_QRData = source.GetDataClone();
            m_rowsCount = source.RowsCount;
            m_colsCount = source.ColsCount;
            m_RDiagData = new double[m_colsCount];

            // Main loop.
            for(int k = 0; k < m_colsCount; k++)
            {
                //Compute 2-norm of k-th column
                double norm = 0;
                for (int i = k; i < m_rowsCount; i++)
                {
                    norm = Hypotenuse(norm, m_QRData[i][k]);
                }

                if (norm != 0)
                {
                    // Form k-th Householder vector.
                    if (m_QRData[k][k] < 0)
                    {
                        norm = -norm;
                    }
                    for (int i = k; i < m_rowsCount; i++)
                    {
                        m_QRData[i][k] /= norm;
                    }
                    m_QRData[k][k] += 1.0;

                    //Apply transformation to remaining columns.
                    Parallel.For(k + 1, m_colsCount, j =>
                    //for (int j = k + 1; j < m_colsCount; j++)
                    {
                        double s = 0.0;
                        for (int i = k; i < m_rowsCount; i++)
                        {
                            s += m_QRData[i][k] * m_QRData[i][j];
                        }
                        s = -s / m_QRData[k][k];
                        for (int i = k; i < m_rowsCount; i++)
                        {
                            m_QRData[i][j] += s * m_QRData[i][k];
                        }
                    });
                }
                m_RDiagData[k] = -norm;
            }
            if (!FullRank)
            {
                throw new Exception("Matrix is rank deficient.");
            }
            return;
        }

        //Properties
        /// <summary>
        /// Is full rank? 
        /// </summary>
        public bool FullRank
        {
            get
            {
                for (int j = 0; j < m_colsCount; j++)
                {
                    if (m_RDiagData[j] == 0)
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
        /// <param name="B">A Matrix with as many rows as A and at least one column.</param>
        public Matrix Solve(Matrix B)
        {
            if (B.RowsCount != m_rowsCount)
            {
                throw new Exception("Different row counts.");
            }

            // Copy right hand side
            int nx = B.ColsCount;
            double[][] X = B.GetDataClone();

            // Compute Y = transpose(Q)*B
            for (int k = 0; k < m_colsCount; k++)
            {
                for (int j = 0; j < nx; j++)
                {
                    double s = 0.0;
                    for (int i = k; i < m_rowsCount; i++)
                    {
                        s += m_QRData[i][k] * X[i][j];
                    }
                    s = -s / m_QRData[k][k];
                    for (int i = k; i < m_rowsCount; i++)
                    {
                        X[i][j] += s * m_QRData[i][k];
                    }
                }
            }
            // Solve R*X = Y;
            for (int k = m_colsCount - 1; k >= 0; k--)
            {
                for (int j = 0; j < nx; j++)
                {
                    X[k][j] /= m_RDiagData[k];
                }
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        X[i][j] -= X[k][j] * m_QRData[i][k];
                    }
                }
            }
            return (new Matrix(X).GetSubMatrix(0, m_colsCount - 1, 0, nx - 1));
        }
    }//QRD

}//Namespace
