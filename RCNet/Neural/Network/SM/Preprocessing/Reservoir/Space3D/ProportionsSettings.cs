using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RCNet.Extensions;
using RCNet.XmlTools;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D
{
    /// <summary>
    /// Class contains 3D dimensions
    /// </summary>
    [Serializable]
    public class ProportionsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ProportionsType";

        /// <summary>
        /// X dimension
        /// </summary>
        public int DimX { get; }

        /// <summary>
        /// Y dimension
        /// </summary>
        public int DimY { get; }

        /// <summary>
        /// Z dimension
        /// </summary>
        public int DimZ { get; }

        //Constructor
        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="dimX">X-Dimension</param>
        /// <param name="dimY">Y-Dimension</param>
        /// <param name="dimZ">Z-Dimension</param>
        public ProportionsSettings(int dimX,
                                   int dimY,
                                   int dimZ
                                   )
        {
            DimX = dimX;
            DimY = dimY;
            DimZ = dimZ;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public ProportionsSettings(ProportionsSettings source)
            :this(source.DimX, source.DimY, source.DimZ)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing the settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ProportionsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            DimX = int.Parse(settingsElem.Attribute("dimX").Value, CultureInfo.InvariantCulture);
            DimY = int.Parse(settingsElem.Attribute("dimY").Value, CultureInfo.InvariantCulture);
            DimZ = int.Parse(settingsElem.Attribute("dimZ").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Total size
        /// </summary>
        public int Size { get { return DimX * DimY * DimZ; } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }


        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (DimX < 1)
            {
                throw new ArgumentException($"Invalid DimX {DimX.ToString(CultureInfo.InvariantCulture)}. DimX must be GE to 1.", "DimX");
            }
            if (DimY < 1)
            {
                throw new ArgumentException($"Invalid DimY {DimY.ToString(CultureInfo.InvariantCulture)}. DimY must be GE to 1.", "DimY");
            }
            if (DimZ < 1)
            {
                throw new ArgumentException($"Invalid DimZ {DimZ.ToString(CultureInfo.InvariantCulture)}. DimZ must be GE to 1.", "DimZ");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ProportionsSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("dimX", DimX.ToString(CultureInfo.InvariantCulture)),
                                                           new XAttribute("dimY", DimY.ToString(CultureInfo.InvariantCulture)),
                                                           new XAttribute("dimZ", DimZ.ToString(CultureInfo.InvariantCulture)));
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("proportions", suppressDefaults);
        }

    }//ProportionsSettings

}//Namespace
