using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Helper class for easier management of predictors
    /// </summary>
    public static class PredictorFactory
    {

        /// <summary>
        /// Constructs name of predictor used in xml setup
        /// </summary>
        /// <param name="predictorID">Enumerated predictor identifier</param>
        public static string GetXmlName(PredictorsProvider.PredictorID predictorID)
        {
            string name = predictorID.ToString();
            return name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);
        }

        /// <summary>
        /// Loads appropriate predictor settings instance based on xml element name
        /// </summary>
        /// <param name="elem">Configuration element</param>
        public static IPredictorSettings LoadPredictorSettings(XElement elem)
        {
            if(elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.Activation))
            {
                return new PredictorActivationSettings(elem);
            }
            else if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.ActivationPower))
            {
                return new PredictorActivationPowerSettings(elem);
            }
            else if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.ActivationStatFeature))
            {
                return new PredictorActivationStatFeatureSettings(elem);
            }
            else if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.ActivationRescalledRange))
            {
                return new PredictorActivationRescalledRangeSettings(elem);
            }
            else if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.ActivationLinWAvg))
            {
                return new PredictorActivationLinWAvgSettings(elem);
            }
            else if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.ActivationDiffStatFeature))
            {
                return new PredictorActivationDiffStatFeatureSettings(elem);
            }
            else if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.ActivationDiffRescalledRange))
            {
                return new PredictorActivationDiffRescalledRangeSettings(elem);
            }
            else if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.ActivationDiffLinWAvg))
            {
                return new PredictorActivationDiffLinWAvgSettings(elem);
            }
            else if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.FiringTrace))
            {
                return new PredictorFiringTraceSettings(elem);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates appropriate instance of the predictor computer
        /// </summary>
        /// <param name="cfg">Configuration of the predictor</param>
        public static IPredictor CreatePredictor(IPredictorSettings cfg)
        {
            Type pType = cfg.GetType();
            if(pType == typeof(PredictorActivationSettings))
            {
                return new PredictorActivation((PredictorActivationSettings)cfg);
            }
            else if(pType == typeof(PredictorActivationPowerSettings))
            {
                return new PredictorActivationPower((PredictorActivationPowerSettings)cfg);
            }
            else if (pType == typeof(PredictorActivationStatFeatureSettings))
            {
                return new PredictorActivationStatFeature((PredictorActivationStatFeatureSettings)cfg);
            }
            else if (pType == typeof(PredictorActivationRescalledRangeSettings))
            {
                return new PredictorActivationRescalledRange((PredictorActivationRescalledRangeSettings)cfg);
            }
            else if (pType == typeof(PredictorActivationLinWAvgSettings))
            {
                return new PredictorActivationLinWAvg((PredictorActivationLinWAvgSettings)cfg);
            }
            else if (pType == typeof(PredictorActivationDiffStatFeatureSettings))
            {
                return new PredictorActivationDiffStatFeature((PredictorActivationDiffStatFeatureSettings)cfg);
            }
            else if (pType == typeof(PredictorActivationDiffRescalledRangeSettings))
            {
                return new PredictorActivationDiffRescalledRange((PredictorActivationDiffRescalledRangeSettings)cfg);
            }
            else if (pType == typeof(PredictorActivationDiffLinWAvgSettings))
            {
                return new PredictorActivationDiffLinWAvg((PredictorActivationDiffLinWAvgSettings)cfg);
            }
            else if (pType == typeof(PredictorFiringTraceSettings))
            {
                return new PredictorFiringTrace((PredictorFiringTraceSettings)cfg);
            }
            else
            {
                throw new ArgumentException($"Unsupported type of predictor settings class {pType.Name}.", "cfg");
            }
        }

    }//PredictorFactory
}
