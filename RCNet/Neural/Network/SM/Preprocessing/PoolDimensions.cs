using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.Neural.Network.SM.Preprocessing
{
    /// <summary>
    /// Class represents dimensions of the neural pool
    /// </summary>
    [Serializable]
    public class PoolDimensions
    {
        /// <summary>
        /// Coordinates (x,y,z) of the pool
        /// </summary>
        public int[] Coordinates { get; }

        /// <summary>
        /// Dimensions (x,y,z) of the pool
        /// </summary>
        public int[] Dimensions { get; }
        
        /// <summary>
        /// Total size
        /// </summary>
        public int Size { get; }

        //Constructor
        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="x">X-Coordinate</param>
        /// <param name="y">Y-Coordinate</param>
        /// <param name="z">Z-Coordinate</param>
        /// <param name="dimX">X-Dimension</param>
        /// <param name="dimY">Y-Dimension</param>
        /// <param name="dimZ">Z-Dimension</param>
        public PoolDimensions(int x, int y, int z, int dimX, int dimY, int dimZ)
        {
            Coordinates = new int[3];
            Coordinates[0] = x;
            Coordinates[1] = y;
            Coordinates[2] = z;
            Dimensions = new int[3];
            Dimensions[0] = dimX;
            Dimensions[1] = dimY;
            Dimensions[2] = dimZ;
            Size = dimX * dimY * dimZ;
            return;
        }

        //Properties
        /// <summary>
        /// X coordinate
        /// </summary>
        public int X { get { return Coordinates[0]; } }

        /// <summary>
        /// Y coordinate
        /// </summary>
        public int Y { get { return Coordinates[1]; } }

        /// <summary>
        /// Z coordinate
        /// </summary>
        public int Z { get { return Coordinates[2]; } }

        /// <summary>
        /// X dimension
        /// </summary>
        public int DimX { get { return Dimensions[0]; } }
        
        /// <summary>
        /// Y dimension
        /// </summary>
        public int DimY { get { return Dimensions[1]; } }
        
        /// <summary>
        /// Z dimension
        /// </summary>
        public int DimZ { get { return Dimensions[2]; } }

        //Static methods
        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            PoolDimensions cmpSettings = obj as PoolDimensions;
            if (!Coordinates.ContainsEqualValues(cmpSettings.Coordinates) ||
                !Dimensions.ContainsEqualValues(cmpSettings.Dimensions)
                )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }//PoolDimensions

}//Namespace
