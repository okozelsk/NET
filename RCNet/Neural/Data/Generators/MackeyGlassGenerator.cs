﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Implements the Mackey-Glass generator.
    /// </summary>
    [Serializable]
    public class MackeyGlassGenerator : IGenerator
    {
        //Constants
        private static readonly double[] IniValues = { 0.9697, 0.9699, 0.9794, 1.0003, 1.0319, 1.0703, 1.1076, 1.1352, 1.1485, 1.1482, 1.1383, 1.1234, 1.1072, 1.0928, 1.0820, 1.0756, 1.0739, 1.0759 };

        //Attributes
        private List<double> _lastValues;
        private readonly MackeyGlassGeneratorSettings _cfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">The configuration.</param>
        public MackeyGlassGenerator(MackeyGlassGeneratorSettings cfg)
        {
            _cfg = (MackeyGlassGeneratorSettings)cfg.DeepClone();
            Reset();
            return;
        }

        //Methods
        /// <inheritdoc />
        public void Reset()
        {
            _lastValues = IniValues.ToList();
            return;
        }

        /// <inheritdoc />
        public double Next()
        {
            double refMGV = _lastValues[_lastValues.Count - _cfg.Tau];
            double lastMGV = _lastValues.Last();
            double nextMGV = lastMGV - _cfg.B * lastMGV + _cfg.C * refMGV / (1 + Math.Pow(refMGV, 10));
            _lastValues.RemoveAt(0);
            _lastValues.Add(nextMGV);
            return nextMGV;
        }

    }//MackeyGlassGenerator

}//Namespace
