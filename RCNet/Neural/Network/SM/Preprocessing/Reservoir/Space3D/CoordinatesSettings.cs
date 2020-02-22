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
    /// Class contains 3D coordinates
    /// </summary>
    [Serializable]
    public class CoordinatesSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "CoordinatesType";
        //Default values
        /// <summary>
        /// Default X coordinate
        /// </summary>
        public const int DefaultX = 0;
        /// <summary>
        /// Default Y coordinate
        /// </summary>
        public const int DefaultY = 0;
        /// <summary>
        /// Default Z coordinate
        /// </summary>
        public const int DefaultZ = 0;

        /// <summary>
        /// X coordinate
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y coordinate
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Z coordinate
        /// </summary>
        public int Z { get; }

        //Constructor
        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="x">X-Coordinate</param>
        /// <param name="y">Y-Coordinate</param>
        /// <param name="z">Z-Coordinate</param>
        public CoordinatesSettings(int x = DefaultX,
                                 int y = DefaultY,
                                 int z = DefaultZ
                                 )
        {
            X = x;
            Y = y;
            Z = z;
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public CoordinatesSettings(CoordinatesSettings source)
            :this(source.X, source.Y, source.Z)
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
        public CoordinatesSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            X = int.Parse(settingsElem.Attribute("x").Value, CultureInfo.InvariantCulture);
            Y = int.Parse(settingsElem.Attribute("y").Value, CultureInfo.InvariantCulture);
            Z = int.Parse(settingsElem.Attribute("z").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultX { get { return (X == DefaultX); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultY { get { return (Y == DefaultY); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultZ { get { return (Z == DefaultZ); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultX && IsDefaultY && IsDefaultZ; } }


        //Methods
        /// <summary>
        /// Returns x,y,z coordinates in the array
        /// </summary>
        public int[] GetCoordinates()
        {
            return new int[] { X, Y, Z };
        }
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new CoordinatesSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("coordinates", suppressDefaults);
        }

    }//PlacementSettings

}//Namespace
