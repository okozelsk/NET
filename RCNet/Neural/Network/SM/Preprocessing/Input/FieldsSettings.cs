using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools.Probability;
using RCNet.XmlTools;
using RCNet.RandomValue;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Contains configuration of external and internal input fields
    /// </summary>
    [Serializable]
    public class FieldsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPInpFieldsType";

        //Attribute properties
        /// <summary>
        /// External input fields settings
        /// </summary>
        public ExternalFieldsSettings ExternalFieldsCfg { get; }

        /// <summary>
        /// Internal input fields settings
        /// </summary>
        public InternalFieldsSettings InternalFieldsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="externalFieldsCfg">External input fields settings</param>
        /// <param name="internalFieldsCfg">Internal input fields settings</param>
        public FieldsSettings(ExternalFieldsSettings externalFieldsCfg,
                              InternalFieldsSettings internalFieldsCfg = null
                              )
        {
            ExternalFieldsCfg = (ExternalFieldsSettings)externalFieldsCfg.DeepClone();
            InternalFieldsCfg = internalFieldsCfg == null ? null : (InternalFieldsSettings)internalFieldsCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public FieldsSettings(FieldsSettings source)
            :this(source.ExternalFieldsCfg, source.InternalFieldsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
        public FieldsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ExternalFieldsCfg = new ExternalFieldsSettings(settingsElem.Descendants("externalFields").First());
            XElement internalFieldsElem = settingsElem.Descendants("internalFields").FirstOrDefault();
            InternalFieldsCfg = internalFieldsElem == null ? null : new InternalFieldsSettings(internalFieldsElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Total number of fields (external + internal)
        /// </summary>
        public int TotalNumOfFields { get { return ExternalFieldsCfg.FieldCfgCollection.Count + (InternalFieldsCfg == null ? 0 : InternalFieldsCfg.FieldCfgCollection.Count); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (InternalFieldsCfg != null)
            {
                foreach(InternalFieldSettings internalFieldCfg in InternalFieldsCfg.FieldCfgCollection)
                {
                    if(ExternalFieldsCfg.GetFieldID(internalFieldCfg.Name, false) != -1)
                    {
                        throw new Exception($"Internal field name {internalFieldCfg.Name} found among external fields.");
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Returns the zero-based index of the field among concated external and internal fields or -1 if name was not found.
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="ex">Specifies if to throw exception when not found</param>
        public int GetFieldID(string fieldName, bool ex = true)
        {
            int index = ExternalFieldsCfg.GetFieldID(fieldName, false);
            if(index != -1)
            {
                return index;
            }
            if (InternalFieldsCfg != null)
            {
                index = InternalFieldsCfg.GetFieldID(fieldName, false);
                if (index != -1)
                {
                    return ExternalFieldsCfg.FieldCfgCollection.Count + index;
                }
            }
            if (ex)
            {
                throw new Exception($"Field name {fieldName} not found.");
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Returns configuration of the given field
        /// </summary>
        /// <param name="fieldName">Field name</param>
        public RCNetBaseSettings GetFieldCfg(string fieldName)
        {
            int index = ExternalFieldsCfg.GetFieldID(fieldName, false);
            if (index != -1)
            {
                return ExternalFieldsCfg.FieldCfgCollection[index];
            }
            if (InternalFieldsCfg != null)
            {
                index = InternalFieldsCfg.GetFieldID(fieldName, false);
                if (index != -1)
                {
                    return InternalFieldsCfg.FieldCfgCollection[index];
                }
            }
            throw new Exception($"Field name {fieldName} not found.");
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new FieldsSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             ExternalFieldsCfg.GetXml(suppressDefaults)
                                             );
            if(InternalFieldsCfg != null)
            {
                rootElem.Add(InternalFieldsCfg.GetXml(suppressDefaults));
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
            return GetXml("fields", suppressDefaults);
        }

    }//FieldsSettings

}//Namespace
