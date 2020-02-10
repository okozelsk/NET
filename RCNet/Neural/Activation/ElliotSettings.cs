using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.RandomValue;
using RCNet.XmlTools;
using RCNet.MathTools.Differential;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Class encaptulates arguments of the Elliot activation function.
    /// Arguments are in RandomValue form to allow their dynamic random initialization within the specified ranges.
    /// </summary>
    [Serializable]
    public class ElliotSettings : RCNetBaseSettings
    {
        //Constants
        public const string XsdTypeName = "ActivationElliotCfgType";

        //Typical values
        /// <summary>
        /// Curve slope
        /// </summary>
        public const double TypicalSlope = 1;

        //Attribute properties
        /// <summary>
        /// Slope of the curve
        /// </summary>
        public RandomValueSettings Slope { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="slope">Slope of the curve</param>
        public ElliotSettings(RandomValueSettings slope = null)
        {
            Slope = RandomValueSettings.CloneOrDefault(slope, TypicalSlope);
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ElliotSettings(ElliotSettings source)
        {
            Slope = source.Slope.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing Elliot activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ElliotSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Slope = RandomValueSettings.LoadOrDefault(activationSettingsElem, "slope", TypicalSlope);
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public ElliotSettings DeepClone()
        {
            ElliotSettings clone = new ElliotSettings(this);
            return clone;
        }

    }//ElliotSettings

}//Namespace
