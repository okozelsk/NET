using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D
{
    /// <summary>
    /// Configuration of the 3D coordinates.
    /// </summary>
    [Serializable]
    public class CoordinatesSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "CoordinatesType";
        //Default values
        /// <summary>
        /// The default X coordinate.
        /// </summary>
        public const int DefaultX = 0;
        /// <summary>
        /// The default Y coordinate.
        /// </summary>
        public const int DefaultY = 0;
        /// <summary>
        /// The default Z coordinate.
        /// </summary>
        public const int DefaultZ = 0;

        /// <summary>
        /// The X coordinate.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// The Y coordinate.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// The Z coordinate.
        /// </summary>
        public int Z { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public CoordinatesSettings(int x = DefaultX,
                                   int y = DefaultY,
                                   int z = DefaultZ
                                   )
        {
            X = x;
            Y = y;
            Z = z;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public CoordinatesSettings(CoordinatesSettings source)
            : this(source.X, source.Y, source.Z)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public CoordinatesSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            X = int.Parse(settingsElem.Attribute("x").Value, CultureInfo.InvariantCulture);
            Y = int.Parse(settingsElem.Attribute("y").Value, CultureInfo.InvariantCulture);
            Z = int.Parse(settingsElem.Attribute("z").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultX { get { return (X == DefaultX); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultY { get { return (Y == DefaultY); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultZ { get { return (Z == DefaultZ); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultX &&
                       IsDefaultY &&
                       IsDefaultZ;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <summary>
        /// Returns x,y,z coordinates in the array
        /// </summary>
        public int[] GetCoordinates()
        {
            return new int[] { X, Y, Z };
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new CoordinatesSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultX)
            {
                rootElem.Add(new XAttribute("x", X.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultY)
            {
                rootElem.Add(new XAttribute("y", Y.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultZ)
            {
                rootElem.Add(new XAttribute("z", Z.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("coordinates", suppressDefaults);
        }

    }//CoordinatesSettings

}//Namespace
