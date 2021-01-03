using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using System;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.PM
{
    /// <summary>
    /// Configuration of the predictors mapper's allowed predictor.
    /// </summary>
    [Serializable]
    public class AllowedPredictorSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SMMapperAllowedPredictorType";

        //Attribute properties
        /// <inheritdoc cref="PredictorsProvider.PredictorID"/>
        public PredictorsProvider.PredictorID PredictorID { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="predictorID">An identifier of the predictor.</param>
        public AllowedPredictorSettings(PredictorsProvider.PredictorID predictorID)
        {
            PredictorID = predictorID;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public AllowedPredictorSettings(AllowedPredictorSettings source)
            : this(source.PredictorID)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public AllowedPredictorSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            PredictorID = (PredictorsProvider.PredictorID)Enum.Parse(typeof(PredictorsProvider.PredictorID), settingsElem.Attribute("name").Value);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }


        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new AllowedPredictorSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("name", PredictorID.ToString())
                                             );
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("predictor", suppressDefaults);
        }


    }//AllowedPredictorSettings

}//Namespace
