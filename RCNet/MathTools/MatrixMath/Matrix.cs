using System;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.MathTools.MatrixMath
{
    /// <summary>
    /// Class represents the mathematical matrix of double values.
    /// Class does not support the sparse matrix format.
    /// </summary>
    [Serializable]
    public class Matrix
    {
        //Constants
        //Attributes
        /// <summary>
        /// Matrix data stored in array of arrays of double.
        /// </summary>
        protected double[][] _data;

        //Constructors
        /// <summary>
        /// Instantiates the matrix.
        /// </summary>
        /// <param name="numOfRows">Number of rows</param>
        /// <param name="numOfCols">Number of columns</param>
        /// <param name="flatData">Optional. The data to be copied into the matrix (in the flat form)</param>
        public Matrix(int numOfRows, int numOfCols, double[] flatData = null)
        {
            _data = new double[numOfRows][];
            Parallel.For(0, numOfRows, row =>
            {
                _data[row] = new double[numOfCols];
                _data[row].Populate(0);
            });
            if (flatData != null)
            {
                Set(flatData);
            }
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="sourceMatrix"></param>
        public Matrix(Matrix sourceMatrix)
            :this(sourceMatrix._data)
        {
            return;
        }

        /// <summary>
        /// Instantiates matrix based on dimensions of given array of arrays and copies the data into the new matrix.
        /// </summary>
        public Matrix(double[][] data)
        {
            _data = data.Clone2D();
            return;
        }

        //Properties
        /// <summary>
        /// Matrix data
        /// </summary>
        public double[][] Data { get { return _data; } }
        /// <summary>
        /// Number of matrix rows
        /// </summary>
        public int NumOfRows { get { return _data.Length; } }
        /// <summary>
        /// Number of matrix columns
        /// </summary>
        public int NumOfCols { get { return _data[0].Length; } }
        /// <summary>
        /// Size of the matrix = (NumOfRows * NumOfCols)
        /// </summary>
        public int Size { get { return NumOfRows * NumOfCols; } }
        /// <summary>
        /// Indicates whether the matrix is a vector.
        /// </summary>
        public bool IsVector { get { return (NumOfRows == 1 || NumOfCols == 1); } }
        /// <summary>
        /// Indicates whether the matrix is the singular matrix.
        /// </summary>
        public bool IsSingular
        {
            get
            {
                for(int i = 0; i < NumOfRows; i++)
                {
                    for(int j = 0; j < NumOfCols; j++)
                    {
                        if(_data[i][j] != 0)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Checkes if the matrix is squared (equal number of rows and columns)
        /// </summary>
        public bool IsSquared
        {
            get
            {
                return (NumOfCols == NumOfRows);
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
            double hypot = 0d;
            if (Math.Abs(x) > Math.Abs(y))
            {
                hypot = Math.Abs(x) * Math.Sqrt(1d + (y / x).Power(2));
            }
            else if (y != 0)
            {
                hypot = Math.Abs(y) * Math.Sqrt(1d + (x / y).Power(2));
            }
            return hypot;
        }

        /// <summary>
        /// Creates a matrix that has data.Length rows and one column.
        /// </summary>
        /// <param name="data">Column values</param>
        public static Matrix CreateSingleColumnMatrix(double[] data)
        {
            Matrix result = new Matrix(data.Length, 1);
            result.SetCol(0, data);
            return result;
        }

        /// <summary>
        /// Creates a matrix that has one row and data.Length columns.
        /// </summary>
        /// <param name="data">Row values</param>
        public static Matrix CreateSingleRowMatrix(double[] data)
        {
            Matrix result = new Matrix(1, data.Length);
            result.SetRow(0, data);
            return result;
        }

        //Instance methods
        /// <summary>
        /// Fills the whole matrix with given value
        /// </summary>
        /// <param name="value">Value</param>
        public void Set(double value = 0)
        {
            _data.Populate(value);
            return;
        }

        /// <summary>
        /// Fills the whole matrix with the values from given 1D array (flat format)
        /// </summary>
        /// <param name="flatData">Data in the flat format</param>
        public void Set(double[] flatData)
        {
            int dataIndex = 0;
            for (int i = 0; i < NumOfRows; i++)
            {
                for (int j = 0; j < NumOfCols; j++)
                {
                    _data[i][j] = flatData[dataIndex++];
                }
            }
            return;
        }

        /// <summary>
        /// Copies all values from the source matrix into the this matrix.
        /// </summary>
        /// <param name="source">Source matrix</param>
        public void Set(Matrix source)
        {
            Parallel.For(0, NumOfRows, i =>
            {
                for (int j = 0; j < NumOfCols; j++)
                {
                    _data[i][j] = source._data[i][j];
                }
            });
            return;
        }

        /// <summary>
        /// Fills specified row with given value
        /// </summary>
        /// <param name="row">Matrix row</param>
        /// <param name="value">Value</param>
        public void SetRow(int row, double value = 0)
        {
            _data[row].Populate(value);
            return;
        }

        /// <summary>
        /// Copies values from the given array into the specified matrix row.
        /// </summary>
        /// <param name="row">Matrix row</param>
        /// <param name="data">Data</param>
        public void SetRow(int row, double[] data)
        {
            data.CopyTo(_data[row], 0);
            return;
        }

        /// <summary>
        /// Fills specified matrix column with the specified value.
        /// </summary>
        /// <param name="col">Matrix column</param>
        /// <param name="value">Value</param>
        public void SetCol(int col, double value = 0)
        {
            for(int i = 0; i < _data.Length; i++)
            {
                _data[i][col] = value;
            }
            return;
        }

        /// <summary>
        /// Copies values from the given array into the specified matrix column.
        /// </summary>
        /// <param name="col">Matrix column</param>
        /// <param name="data">Data</param>
        public void SetCol(int col, double[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                _data[i][col] = data[i];
            }
            return;
        }

        /// <summary>
        /// Copies values from the source matrix into the this matrix starting from specified row and col position.
        /// </summary>
        /// <param name="fromRow">Initial row index</param>
        /// <param name="fromCol">Initial column index</param>
        /// <param name="source">Source matrix</param>
        public void SetSubMatrix(int fromRow, int fromCol, Matrix source)
        {
            for (int i = 0; i <= source.NumOfRows; i++)
            {
                for (int j = 0; j <= source.NumOfCols; j++)
                {
                    _data[fromRow + i][fromCol + j] = source._data[i][j];
                }
            }
            return;
        }

        /// <summary>
        /// Copies data of this matrix into the 1D array of double values (flat format)
        /// </summary>
        /// <param name="flatData">Array to be filled</param>
        public void GetFlatData(double[] flatData)
        {
            int dataIndex = 0;
            for (int i = 0; i < NumOfRows; i++)
            {
                for (int j = 0; j < NumOfCols; j++)
                {
                    flatData[dataIndex++] = _data[i][j];
                }
            }
            return;
        }

        /// <summary>
        /// Converts data of this matrix into the 1D array of double values (flat format)
        /// </summary>
        public double[] GetFlatData()
        {
            double[] flatData = new double[Size];
            GetFlatData(flatData);
            return flatData;
        }

        /// <summary>
        /// Creates a matrix having the same number of rows as this matrix and one column.
        /// Column values are copied from the specified column of this matrix.
        /// </summary>
        /// <param name="col">This matrix column index</param>
        public Matrix GetColSubMatrix(int col)
        {
            double[][] colMatrixData = new double[NumOfRows][];
            for (int row = 0; row < NumOfRows; row++)
            {
                colMatrixData[row] = new double[1];
                colMatrixData[row][0] = _data[row][col];
            }
            return new Matrix(colMatrixData);
        }

        /// <summary>
        /// Creates a matrix having the same number of columns as this matrix and one row.
        /// Row values are copied from the specified row of this matrix.
        /// </summary>
        /// <param name="row">This matrix row index</param>
        public Matrix GetRowSubMatrix(int row)
        {
            double[][] rowMatrixData = new double[1][];
            rowMatrixData[0] = new double[NumOfCols];
            for (int col = 0; col < NumOfCols; col++)
            {
                rowMatrixData[0][col] = _data[row][col];
            }
            return new Matrix(rowMatrixData);
        }

        /// <summary>
        /// Creates a submatrix of this matrix.
        /// </summary>
        /// <param name="fromRow">Initial row index.</param>
        /// <param name="toRow">Final row index.</param>
        /// <param name="fromCol">Initial column index.</param>
        /// <param name="toCol">Final column index.</param>
        public Matrix GetSubMatrix(int fromRow, int toRow, int fromCol, int toCol)
        {
            Matrix resultMatrix = new Matrix(toRow - fromRow + 1, toCol - fromCol + 1);
            for (int i = fromRow; i <= toRow; i++)
            {
                for (int j = fromCol; j <= toCol; j++)
                {
                    resultMatrix._data[i - fromRow][j - fromCol] = _data[i][j];
                }
            }
            return resultMatrix;
        }

        /// <summary>
        /// Returns clone of the internal array of arrays.
        /// </summary>
        public double[][] GetDataClone()
        {
            return _data.Clone2D();
        }

        /// <summary>
        /// Creates the deep copy
        /// </summary>
        public Matrix DeepClone()
        {
            return new Matrix(this);
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Matrix cmpMatrix = obj as Matrix;
            if (NumOfCols != cmpMatrix.NumOfCols || NumOfRows != cmpMatrix.NumOfRows)
            {
                return false;
            }
            for(int i = 0; i < NumOfRows; i++)
            {
                for(int j = 0; j < NumOfCols; j++)
                {
                    if(_data[i][j] != cmpMatrix._data[i][j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Method uses power iteration method to estimate largest eigen value (in magnitude) and corresponding eigen vector.
        /// Matrix must be squared.
        /// </summary>
        /// <param name="resultEigenVector">Returned corresponding eigen vector</param>
        /// <param name="maxNumOfIterations">Maximum number of method's iterations</param>
        /// <param name="stopDelta">Stopping corvengence delta between previous iteration and current iteration</param>
        /// <returns>Estimated largest eigen value (in magnitude)</returns>
        public double EstimateLargestEigenValue(out double[] resultEigenVector, int maxNumOfIterations = 1000, double stopDelta = 1e-6)
        {
            //Firstly check squared matrix
            if (!IsSquared)
            {
                throw new Exception("Matrix must be squared.");
            }
            //Local variables
            //Iteration initialization
            int iteration = 0;
            double iterationDelta = 0;
            int n = NumOfRows;
            double[] tmpVector = new double[n];
            double eigenValue = 0;
            double[] eigenVector = new double[n];
            eigenVector.Populate(1);
            //Results
            double minDelta = double.MaxValue;
            double resultEigenValue = 0;
            resultEigenVector = new double[n];
            //Convergence loop
            do
            {
                Parallel.For(0, n, i =>
                {
                    tmpVector[i] = 0;
                    for (int j = 0; j < n; j++)
                    {
                        tmpVector[i] += _data[i][j] * eigenVector[j];
                    }
                });

                //Find element having max magnitude (= new eigen value)
                double prevEigenValue = eigenValue;
                eigenValue = tmpVector[0];
                for (int i = 1; i < n; i++)
                {
                    if (Math.Abs(tmpVector[i]) > Math.Abs(eigenValue))
                    {
                        eigenValue = tmpVector[i];
                    }
                }

                //Prepare new normalized eigenVector
                for (int i = 0; i < n; i++)
                {
                    eigenVector[i] = tmpVector[i] / eigenValue;
                }

                //Iteration results
                ++iteration;
                iterationDelta = Math.Abs(eigenValue - prevEigenValue);
                if(minDelta > iterationDelta)
                {
                    minDelta = iterationDelta;
                    resultEigenValue = eigenValue;
                    eigenVector.CopyTo(resultEigenVector, 0);
                }

            } while(iteration < maxNumOfIterations && iterationDelta > stopDelta);
            return resultEigenValue;
        }

        /// <summary>
        /// Method uses LU decomposition to solve system of the linear equations.
        /// Matrix must be squared.
        /// </summary>
        /// <param name="rightPart">Desired results (right part of linear equations)</param>
        /// <returns>Linear coefficients</returns>
        public double[] SolveUsingLU(double[] rightPart)
        {
            //Firstly check squared matrix
            if (!IsSquared)
            {
                throw new Exception("Matrix must be squared.");
            }
            int n = NumOfRows;
            double[,] lu = new double[n, n];
            double sum = 0;
            //LU decomposition
            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    sum = 0;
                    for (int k = 0; k < i; k++)
                    {
                        sum += lu[i, k] * lu[k, j];
                    }
                    lu[i, j] = _data[i][j] - sum;
                }
                for (int j = i + 1; j < n; j++)
                {
                    sum = 0;
                    for (int k = 0; k < i; k++)
                    {
                        sum += lu[j, k] * lu[k, i];
                    }
                    lu[j, i] = (1 / lu[i, i]) * (_data[j][i] - sum);
                }
            }
            //Find solution of Ly = b
            double[] y = new double[n];
            for (int i = 0; i < n; i++)
            {
                sum = 0;
                for (int k = 0; k < i; k++)
                {
                    sum += lu[i, k] * y[k];
                }
                y[i] = rightPart[i] - sum;
            }
            // find solution of Ux = y
            double[] x = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                sum = 0;
                for (int k = i + 1; k < n; k++)
                {
                    sum += lu[i, k] * x[k];
                }
                x[i] = (1 / lu[i, i]) * (y[i] - sum);
            }
            return x;
        }

    }//Matrix

} //Namespace

