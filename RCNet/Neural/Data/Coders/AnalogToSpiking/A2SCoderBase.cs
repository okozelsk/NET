using System;
using System.Collections.Generic;
using System.Text;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Common base class for an analog value to a spike-code coders
    /// </summary>
    [Serializable]
    public abstract class A2SCoderBase
    {

        //Attributes

        //Constructor
        /// <summary>
        /// Protected constructor
        /// </summary>
        /// <param name="absValCodeLength">The length of the spike-code of the analog absolute value</param>
        /// <param name="halved">Specifies if to generate halved spike-code (one half for bellow average values and second half for above average values)</param>
        protected A2SCoderBase(int absValCodeLength, bool halved)
        {
            AbsValCodeLength = absValCodeLength;
            Halved = halved;
            CodeTotalLength = AbsValCodeLength * (halved ? 2 : 1);
            return;
        }

        //Attribute properties
        /// <summary>
        /// The length of the spike-code of the analog absolute value
        /// </summary>
        public int AbsValCodeLength { get; }

        /// <summary>
        /// Specifies if to generate halved spike-code where one half is dedicated for bellow average values (-)
        /// and second half for above average values (+)
        /// </summary>
        public bool Halved { get; }

        /// <summary>
        /// Gets a total length of the resulting spike-code
        /// </summary>
        public int CodeTotalLength { get; }

        //Methods
        /// <summary>
        /// Resets coder
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Codes an analog value to the spike-code
        /// </summary>
        /// <param name="normalizedValue">A normalized analog value between -1 and 1</param>
        /// <returns>Resulting spike-code as an array of 0/1 byte values</returns>
        public abstract byte[] GetCode(double normalizedValue);


    }//A2SCoderBase
}
