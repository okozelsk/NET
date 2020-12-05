﻿using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Common interface of the predictors configurations
    /// </summary>
    public interface IPredictorSettings
    {

        /// <inheritdoc cref="PredictorsProvider.PredictorID"/>
        PredictorsProvider.PredictorID ID { get; }

        /// <summary>
        /// Specifies necessary size of the windowed history of activations
        /// </summary>
        int RequiredWndSizeOfActivations { get; }

        /// <summary>
        /// Specifies necessary size of the windowed history of firings
        /// </summary>
        int RequiredWndSizeOfFirings { get; }

        /// <summary>
        /// Indicates use of continuous stat of activations
        /// </summary>
        bool NeedsContinuousActivationStat { get; }

        /// <summary>
        /// Indicates use of continuous stat of activation differences
        /// </summary>
        bool NeedsContinuousActivationDiffStat { get; }

    }//IPredictorSettings

}//Namespace
