using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Class represents neuron placement within the pool
    /// </summary>
    [Serializable]
    public class NeuronPlacement
    {
        /// <summary>
        /// Neuron's index in a reservoir flat stucture
        /// </summary>
        public int ReservoirFlatIdx { get; }
        /// <summary>
        /// Neuron's home pool ID
        /// </summary>
        public int PoolID { get; }
        /// <summary>
        /// Neuron index in a pool flat stucture
        /// </summary>
        public int PoolFlatIdx { get; }
        /// <summary>
        /// Neuron's group index within the pool
        /// </summary>
        public int GroupID { get; }
        /// <summary>
        /// Coordinates (x,y,z) within the reservoir
        /// </summary>
        public int[] Coordinates { get; }

        //Constructor
        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="reservoirFlatIdx">Index of the neuron in a reservoir flat structure.</param>
        /// <param name="poolID">Home pool index.</param>
        /// <param name="poolFlatIdx">Index of the neuron in a pool flat structure.</param>
        /// <param name="groupID">Index of the neuron's group in a pool.</param>
        /// <param name="x">X coordinate in the reservoir</param>
        /// <param name="y">Y coordinate in the reservoir</param>
        /// <param name="z">Z coordinate in the reservoir</param>
        public NeuronPlacement(int reservoirFlatIdx,
                               int poolID,
                               int poolFlatIdx,
                               int groupID,
                               int x,
                               int y,
                               int z
                               )
        {
            ReservoirFlatIdx = reservoirFlatIdx;
            PoolID = poolID;
            PoolFlatIdx = poolFlatIdx;
            GroupID = groupID;
            Coordinates = new int[3];
            Coordinates[0] = x;
            Coordinates[1] = y;
            Coordinates[2] = z;
            return;
        }
    }//NeuronPlacement

}//Namespace
