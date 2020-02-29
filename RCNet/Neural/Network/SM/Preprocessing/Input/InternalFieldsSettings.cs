﻿using System;
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
    /// Collection of internal field settings
    /// </summary>
    [Serializable]
    public class InternalFieldsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPInternalInpFieldsType";

        //Attribute properties
        /// <summary>
        /// Collection of external input field settings
        /// </summary>
        public List<InternalFieldSettings> FieldCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        private InternalFieldsSettings()
        {
            FieldCfgCollection = new List<InternalFieldSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="fieldCfgCollection">Collection of internal input field settings</param>
        public InternalFieldsSettings(IEnumerable<InternalFieldSettings> fieldCfgCollection)
            : this()
        {
            AddFields(fieldCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="fieldCfgCollection">Collection of internal input field settings</param>
        public InternalFieldsSettings(params InternalFieldSettings[] fieldCfgCollection)
            : this()
        {
            AddFields(fieldCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InternalFieldsSettings(InternalFieldsSettings source)
            : this()
        {
            AddFields(source.FieldCfgCollection);
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
        public InternalFieldsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            FieldCfgCollection = new List<InternalFieldSettings>();
            foreach (XElement fieldElem in settingsElem.Descendants("field"))
            {
                FieldCfgCollection.Add(new InternalFieldSettings(fieldElem));
            }
            Check();
            return;
        }

        //Properties
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
            if (FieldCfgCollection.Count == 0)
            {
                throw new Exception($"At least one internal field configuration must be specified.");
            }
            //Uniqueness of field names
            string[] names = new string[FieldCfgCollection.Count];
            names[0] = FieldCfgCollection[0].Name;
            for(int i = 1; i < FieldCfgCollection.Count; i++)
            {
                if(names.Contains(FieldCfgCollection[i].Name))
                {
                    throw new Exception($"Field name {FieldCfgCollection[i].Name} is not unique.");
                }
                names[i] = FieldCfgCollection[i].Name;
            }
            return;
        }

        /// <summary>
        /// Adds cloned schemas from given collection into the internal collection
        /// </summary>
        /// <param name="fieldCfgCollection">Collection of internal input field settings</param>
        private void AddFields(IEnumerable<InternalFieldSettings> fieldCfgCollection)
        {
            foreach (InternalFieldSettings fieldCfg in fieldCfgCollection)
            {
                FieldCfgCollection.Add((InternalFieldSettings)fieldCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Returns the zero-based index of the field or -1 if name was not found
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="ex">Specifies if to throw exception when not found</param>
        public int GetFieldID(string fieldName, bool ex = true)
        {
            for (int i = 0; i < FieldCfgCollection.Count; i++)
            {
                if (FieldCfgCollection[i].Name == fieldName)
                {
                    return i;
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
        public InternalFieldSettings GetFieldCfg(string fieldName)
        {
            return FieldCfgCollection[GetFieldID(fieldName)];
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InternalFieldsSettings(this);
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
            foreach (InternalFieldSettings fieldCfg in FieldCfgCollection)
            {
                rootElem.Add(fieldCfg.GetXml(suppressDefaults));
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
            return GetXml("internalFields", suppressDefaults);
        }

    }//InternalFieldsSettings

}//Namespace
