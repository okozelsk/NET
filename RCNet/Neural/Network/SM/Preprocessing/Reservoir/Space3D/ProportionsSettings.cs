using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D
{
    /// <summary>
    /// Configuration of the 3D proportions.
    /// </summary>
    [Serializable]
    public class ProportionsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ProportionsType";

        /// <summary>
        /// The X dimension.
        /// </summary>
        public int DimX { get; }

        /// <summary>
        /// The Y dimension.
        /// </summary>
        public int DimY { get; }

        /// <summary>
        /// The Z dimension.
        /// </summary>
        public int DimZ { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="dimX">The X dimension.</param>
        /// <param name="dimY">The Y dimension.</param>
        /// <param name="dimZ">The Z dimension.</param>
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
        /// <param name="source">The source instance.</param>
        public ProportionsSettings(ProportionsSettings source)
            : this(source.DimX, source.DimY, source.DimZ)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// The total size.
        /// </summary>
        public int Size { get { return DimX * DimY * DimZ; } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }


        //Methods
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new ProportionsSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("dimX", DimX.ToString(CultureInfo.InvariantCulture)),
                                                           new XAttribute("dimY", DimY.ToString(CultureInfo.InvariantCulture)),
                                                           new XAttribute("dimZ", DimZ.ToString(CultureInfo.InvariantCulture)));
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("proportions", suppressDefaults);
        }

    }//ProportionsSettings

}//Namespace
