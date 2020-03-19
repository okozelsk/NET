using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron.Predictor
{
    /// <summary>
    /// Firing moving weighted average predictor settings
    /// </summary>
    [Serializable]
    public class FiringMWAvgSettings : MWAvgPredictorSettings, IPredictorParamsSettings
    {
        //Constants

        //Attribute properties
        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="window">Window length</param>
        /// <param name="leakage">Leakage</param>
        /// <param name="weights">Type of weighting</param>
        public FiringMWAvgSettings(int window = DefaultWindow,
                                            int leakage = DefaultLeakage,
                                            PredictorsProvider.PredictorMWAvgWeightsType weights = DefaultWeights
                                            )
            :base(window, leakage, weights)
        {
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public FiringMWAvgSettings(FiringMWAvgSettings source)
            :base(source)
        {
            return;
        }

        /// <summary>
        /// Creates initialized instance using xml element
        /// </summary>
        /// <param name="elem">Xml element containing settings</param>
        public FiringMWAvgSettings(XElement elem)
            :base(elem)
        {
            return;
        }

        //Properties
        /// <summary>
        /// Predictor's ID
        /// </summary>
        public PredictorsProvider.PredictorID ID { get { return PredictorsProvider.PredictorID.FiringMWAvg; } }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new FiringMWAvgSettings(this);
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml(PredictorsSettings.GetXmlName(ID), suppressDefaults);
        }

    }//PredictorFiringMWAvgSettings

}//Namespace
