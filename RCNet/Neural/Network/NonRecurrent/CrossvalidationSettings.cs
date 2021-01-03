using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Configuration of the crossvalidation.
    /// </summary>
    [Serializable]
    public class CrossvalidationSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "CrossvalidationType";
        /// <summary>
        /// The maximum fold data ratio.
        /// </summary>
        public const double MaxFoldDataRatio = 0.5d;
        /// <summary>
        /// The string code for the automatic number of folds.
        /// </summary>
        public const string AutoFoldsCode = "Auto";
        /// <summary>
        /// The numeric code for the automatic number of folds.
        /// </summary>
        public const int AutoFolds = 0;
        //Default values
        /// <summary>
        /// The default value of the parameter specifying the ratio of samples constituting one fold. Default value is 1/10.
        /// </summary>
        public const double DefaultFoldDataRatio = 0.1d;
        /// <summary>
        /// Default code value of the parameter specifying the number of folds to be used. Default value is "Auto" (all available folds).
        /// </summary>
        public const string DefaultFoldsString = AutoFoldsCode;
        /// <summary>
        /// Default numeric value of the parameter specifying the number of folds to be used. Default value is 0 (all available folds).
        /// </summary>
        public const int DefaultFoldsNum = AutoFolds;
        /// <summary>
        /// The default value of the parameter defining how many times the generation of whole folds on shuffled data to be repeated. This parameter multiplies the number of networks in the cluster. Default value is 1.
        /// </summary>
        public const int DefaultRepetitions = 1;

        //Attribute properties
        /// <summary>
        /// Specifies the ratio of samples constituting one fold.
        /// </summary>
        public double FoldDataRatio { get; }

        /// <summary>
        /// Specifies the number of folds to be used.
        /// </summary>
        public int Folds { get; }

        /// <summary>
        /// Defines how many times the generation of whole folds on shuffled data to be repeated. This parameter multiplies the number of networks in the cluster.
        /// </summary>
        public int Repetitions { get; }

        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="foldDataRatio">Specifies the ratio of samples constituting one fold.</param>
        /// <param name="folds">Specifies the number of folds to be used.</param>
        /// <param name="repetitions">Defines how many times the generation of whole folds on shuffled data to be repeated. This parameter multiplies the number of networks in the cluster.</param>
        public CrossvalidationSettings(double foldDataRatio = DefaultFoldDataRatio,
                                       int folds = DefaultFoldsNum,
                                       int repetitions = DefaultRepetitions
                                       )
        {
            FoldDataRatio = foldDataRatio;
            Folds = folds;
            Repetitions = repetitions;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public CrossvalidationSettings(CrossvalidationSettings source)
            : this(source.FoldDataRatio, source.Folds, source.Repetitions)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public CrossvalidationSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            FoldDataRatio = double.Parse(settingsElem.Attribute("foldDataRatio").Value, CultureInfo.InvariantCulture);
            Folds = settingsElem.Attribute("folds").Value == DefaultFoldsString ? DefaultFoldsNum : int.Parse(settingsElem.Attribute("folds").Value, CultureInfo.InvariantCulture);
            Repetitions = int.Parse(settingsElem.Attribute("repetitions").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultFoldDataRatio { get { return (FoldDataRatio == DefaultFoldDataRatio); } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultFolds { get { return (Folds == DefaultFoldsNum); } }
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRepetitions { get { return (Repetitions == DefaultRepetitions); } }
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultFoldDataRatio &&
                       IsDefaultFolds &&
                       IsDefaultRepetitions;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (FoldDataRatio <= 0 || FoldDataRatio > MaxFoldDataRatio)
            {
                throw new ArgumentException($"Invalid FoldDataRatio {FoldDataRatio.ToString(CultureInfo.InvariantCulture)}. TestDataRatio must be GT 0 and GE {MaxFoldDataRatio.ToString(CultureInfo.InvariantCulture)}.", "FoldDataRatio");
            }
            if (Folds < 0)
            {
                throw new ArgumentException($"Invalid Folds {Folds.ToString(CultureInfo.InvariantCulture)}. Folds must be GE to 0 (0 means Auto folds).", "Folds");
            }
            if (Repetitions < 1)
            {
                throw new ArgumentException($"Invalid Repetitions {Repetitions.ToString(CultureInfo.InvariantCulture)}. Repetitions must be GE to 1.", "Repetitions");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new CrossvalidationSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultFoldDataRatio)
            {
                rootElem.Add(new XAttribute("foldDataRatio", FoldDataRatio.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultFolds)
            {
                rootElem.Add(new XAttribute("folds", Folds == DefaultFoldsNum ? DefaultFoldsString : Folds.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultRepetitions)
            {
                rootElem.Add(new XAttribute("repetitions", Repetitions.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("crossvalidation", suppressDefaults);
        }

    }//CrossvalidationSettings

}//Namespace
