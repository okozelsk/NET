using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using RCNet.XmlTools;

namespace RCNet.Neural.Network.PP
{
    /// <summary>
    /// Startup parameters for the parallel perceptron p-delta rule trainer
    /// </summary>
    [Serializable]
    public class PDeltaRuleTrainerSettings
    {
        //Constants
        /// <summary>
        /// Default initial learning rate
        /// </summary>
        public const double DeafaultIniLR = 0.01d;
        /// <summary>
        /// Default learning rate increase
        /// </summary>
        public const double DeafaultIncLR = 1.1d;
        /// <summary>
        /// Default learning rate decrease
        /// </summary>
        public const double DeafaultDecLR = 0.5d;
        /// <summary>
        /// Default learning min rate
        /// </summary>
        public const double DeafaultMinLR = 1E-4;
        /// <summary>
        /// Default learning max rate
        /// </summary>
        public const double DeafaultMaxLR = 0.1d;

        //Attributes
        /// <summary>
        /// Initial learning rate
        /// </summary>
        public double IniLR { get; set; } = DeafaultIniLR;
        /// <summary>
        /// Learning rate increase
        /// </summary>
        public double IncLR { get; set; } = DeafaultIncLR;
        /// <summary>
        /// Learning rate decrease
        /// </summary>
        public double DecLR { get; set; } = DeafaultDecLR;
        /// <summary>
        /// Learning rate minimum
        /// </summary>
        public double MinLR { get; set; } = DeafaultMinLR;
        /// <summary>
        /// Learning rate maximum
        /// </summary>
        public double MaxLR { get; set; } = DeafaultMaxLR;

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="iniLR">Initial learning rate</param>
        /// <param name="incLR">Learning rate increase</param>
        /// <param name="decLR">Learning rate decrease</param>
        /// <param name="minLR">Learning rate minimum</param>
        /// <param name="maxLR">Learning rate maximum</param>
        public PDeltaRuleTrainerSettings(double iniLR = DeafaultIniLR,
                                         double incLR = DeafaultIncLR,
                                         double decLR = DeafaultDecLR,
                                         double minLR = DeafaultMinLR,
                                         double maxLR = DeafaultMaxLR
                                         )
        {
            IniLR = iniLR;
            IncLR = incLR;
            DecLR = decLR;
            MinLR = minLR;
            MaxLR = maxLR;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PDeltaRuleTrainerSettings(PDeltaRuleTrainerSettings source)
        {
            IniLR = source.IniLR;
            IncLR = source.IncLR;
            DecLR = source.DecLR;
            MinLR = source.MinLR;
            MaxLR = source.MaxLR;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing p-delta rule trainer settings</param>
        public PDeltaRuleTrainerSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.PP.PDeltaRuleTrainerSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement pDeltaRuleTrainerSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            IniLR = double.Parse(pDeltaRuleTrainerSettingsElem.Attribute("iniLR").Value, CultureInfo.InvariantCulture);
            IncLR = double.Parse(pDeltaRuleTrainerSettingsElem.Attribute("incLR").Value, CultureInfo.InvariantCulture);
            DecLR = double.Parse(pDeltaRuleTrainerSettingsElem.Attribute("decLR").Value, CultureInfo.InvariantCulture);
            MinLR = double.Parse(pDeltaRuleTrainerSettingsElem.Attribute("minLR").Value, CultureInfo.InvariantCulture);
            MaxLR = double.Parse(pDeltaRuleTrainerSettingsElem.Attribute("maxLR").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            PDeltaRuleTrainerSettings cmpSettings = obj as PDeltaRuleTrainerSettings;
            if (IniLR != cmpSettings.IniLR ||
                IncLR != cmpSettings.IncLR ||
                DecLR != cmpSettings.DecLR ||
                MinLR != cmpSettings.MinLR ||
                MaxLR != cmpSettings.MaxLR
                )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public PDeltaRuleTrainerSettings DeepClone()
        {
            PDeltaRuleTrainerSettings clone = new PDeltaRuleTrainerSettings(this);
            return clone;
        }

    }//PDeltaRuleTrainerSettings

}//Namespace
