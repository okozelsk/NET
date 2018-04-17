using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.Neural.Network.RC.Spiking.MicroCircuit
{
    class MicroCircuitNeuron
    {
        //Attribute properties
        public int[] Coordinates { get; }
        public int FlatIdx { get; }

        //Constructor
        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="x">Zero based X coordinate in a micro circuit</param>
        /// <param name="y">Zero based Y coordinate in a micro circuit</param>
        /// <param name="z">Zero based Z coordinate in a micro circuit</param>
        /// <param name="flatIdx">Zero based index in a 1D micro circuit neurons bank</param>
        public MicroCircuitNeuron(int x, int y, int z, int flatIdx)
        {
            Coordinates = new int[3];
            Coordinates[0] = x;
            Coordinates[1] = y;
            Coordinates[2] = z;
            FlatIdx = flatIdx;
            return;
        }


        //Methods
        public double ComputeEuclideanDistance(MicroCircuitNeuron neuron)
        {
            double sum = 0;
            for(int i = 0; i < Coordinates.Length; i++)
            {
                sum += ((double)(Coordinates[i] - neuron.Coordinates[i])).Power(2);
            }
            return Math.Sqrt(sum);
        }


    }//MicroCircuitNeuron

}//Namespace
