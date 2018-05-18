using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Class represents dimensions of the neural pool
    /// </summary>
    [Serializable]
    public class PoolDimensions
    {
        /// <summary>
        /// Dimensions (x,y,z) of the pool
        /// </summary>
        public int[] Dimensions { get; }
        
        /// <summary>
        /// Total size (=x*y*z)
        /// </summary>
        public int Size { get; }

        //Constructor
        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="x">X-Dimension</param>
        /// <param name="y">Y-Dimension</param>
        /// <param name="z">Z-Dimension</param>
        public PoolDimensions(int x, int y, int z)
        {
            Dimensions = new int[3];
            Dimensions[0] = x;
            Dimensions[1] = y;
            Dimensions[2] = z;
            Size = x * y * z;
            return;
        }

        //Properties
        /// <summary>
        /// X dimension
        /// </summary>
        public int X { get { return Dimensions[0]; } }
        
        /// <summary>
        /// Y dimension
        /// </summary>
        public int Y { get { return Dimensions[1]; } }
        
        /// <summary>
        /// Z dimension
        /// </summary>
        public int Z { get { return Dimensions[2]; } }

        //Methods
        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            PoolDimensions cmpSettings = obj as PoolDimensions;
            if (!Dimensions.ContainsEqualValues(cmpSettings.Dimensions))
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
