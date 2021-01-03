using System;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Provides a proper instantiation of the predictor computers and also proper loading of their configurations.
    /// </summary>
    public static class PredictorFactory
    {

        /// <summary>
        /// Gets the name of the predictor for the use in a xml setup.
        /// </summary>
        /// <param name="predictorID">An identigier of the predictor.</param>
        public static string GetXmlName(PredictorsProvider.PredictorID predictorID)
        {
            string name = predictorID.ToString();
            return name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);
        }

        /// <summary>
        /// Loads the predictor configuration from the specified xml element.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public static IPredictorSettings LoadPredictorSettings(XElement elem)
        {
            if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.Activation))
            {
                return new PredictorActivationSettings(elem);
            }
            else if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.ActivationPower))
            {
                return new PredictorActivationPowerSettings(elem);
            }
            else if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.ActivationStatFigure))
            {
                return new PredictorActivationStatFigureSettings(elem);
            }
            else if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.ActivationRescaledRange))
            {
                return new PredictorActivationRescaledRangeSettings(elem);
            }
            else if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.ActivationLinWAvg))
            {
                return new PredictorActivationLinWAvgSettings(elem);
            }
            else if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.ActivationDiffStatFigure))
            {
                return new PredictorActivationDiffStatFigureSettings(elem);
            }
            else if (elem.Name.LocalName == GetXmlName(PredictorsProvider.PredictorID.ActivationDiffRescaledRange))
            {
                return new PredictorActivationDiffRescaledRangeSettings(elem);
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
        /// Creates an instance of the predictor computer.
        /// </summary>
        /// <param name="cfg">The configuration of the predictor.</param>
        public static IPredictor CreatePredictor(IPredictorSettings cfg)
        {
            Type pType = cfg.GetType();
            if (pType == typeof(PredictorActivationSettings))
            {
                return new PredictorActivation((PredictorActivationSettings)cfg);
            }
            else if (pType == typeof(PredictorActivationPowerSettings))
            {
                return new PredictorActivationPower((PredictorActivationPowerSettings)cfg);
            }
            else if (pType == typeof(PredictorActivationStatFigureSettings))
            {
                return new PredictorActivationStatFigure((PredictorActivationStatFigureSettings)cfg);
            }
            else if (pType == typeof(PredictorActivationRescaledRangeSettings))
            {
                return new PredictorActivationRescaledRange((PredictorActivationRescaledRangeSettings)cfg);
            }
            else if (pType == typeof(PredictorActivationLinWAvgSettings))
            {
                return new PredictorActivationLinWAvg((PredictorActivationLinWAvgSettings)cfg);
            }
            else if (pType == typeof(PredictorActivationDiffStatFigureSettings))
            {
                return new PredictorActivationDiffStatFigure((PredictorActivationDiffStatFigureSettings)cfg);
            }
            else if (pType == typeof(PredictorActivationDiffRescaledRangeSettings))
            {
                return new PredictorActivationDiffRescaledRange((PredictorActivationDiffRescaledRangeSettings)cfg);
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
