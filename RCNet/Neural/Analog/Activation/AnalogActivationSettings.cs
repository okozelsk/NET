using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.XmlTools;

namespace RCNet.Neural.Analog.Activation
{
    /// <summary>
    /// Class specifies properties of randomly generated values
    /// </summary>
    [Serializable]
    public class AnalogActivationSettings
    {
        //Attribute properties
        /// <summary>
        /// Type of activation function
        /// </summary>
        public AnalogActivationFactory.FunctionType FunctionType { get; set; }
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg1 { get; set; }
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg2 { get; set; }
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg3 { get; set; }
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg4 { get; set; }
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg5 { get; set; }
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg6 { get; set; }
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg7 { get; set; }
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg8 { get; set; }
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg9 { get; set; }
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg10 { get; set; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="functionType">Type of activation function</param>
        /// <param name="arg1">Value of the argument to be passed to the activation function constructor</param>
        /// <param name="arg2">Value of the argument to be passed to the activation function constructor</param>
        /// <param name="arg3">Value of the argument to be passed to the activation function constructor</param>
        /// <param name="arg4">Value of the argument to be passed to the activation function constructor</param>
        /// <param name="arg5">Value of the argument to be passed to the activation function constructor</param>
        /// <param name="arg6">Value of the argument to be passed to the activation function constructor</param>
        /// <param name="arg7">Value of the argument to be passed to the activation function constructor</param>
        /// <param name="arg8">Value of the argument to be passed to the activation function constructor</param>
        /// <param name="arg9">Value of the argument to be passed to the activation function constructor</param>
        /// <param name="arg10">Value of the argument to be passed to the activation function constructor</param>
        public AnalogActivationSettings(AnalogActivationFactory.FunctionType functionType,
                                  double arg1 = double.NaN,
                                  double arg2 = double.NaN,
                                  double arg3 = double.NaN,
                                  double arg4 = double.NaN,
                                  double arg5 = double.NaN,
                                  double arg6 = double.NaN,
                                  double arg7 = double.NaN,
                                  double arg8 = double.NaN,
                                  double arg9 = double.NaN,
                                  double arg10 = double.NaN
                                  )
        {
            FunctionType = functionType;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
            Arg6 = arg6;
            Arg7 = arg7;
            Arg8 = arg8;
            Arg9 = arg9;
            Arg10 = arg10;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AnalogActivationSettings(AnalogActivationSettings source)
        {
            FunctionType = source.FunctionType;
            Arg1 = source.Arg1;
            Arg2 = source.Arg2;
            Arg3 = source.Arg3;
            Arg4 = source.Arg4;
            Arg5 = source.Arg5;
            Arg6 = source.Arg6;
            Arg7 = source.Arg7;
            Arg8 = source.Arg8;
            Arg9 = source.Arg9;
            Arg10 = source.Arg10;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing ActivationSettings settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public AnalogActivationSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Analog.Activation.AnalogActivationSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement activationSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            FunctionType = AnalogActivationFactory.ParseActivationFunctionType(activationSettingsElem.Attribute("function").Value);
            Arg1 = GetArgFromXml(activationSettingsElem, 1);
            Arg2 = GetArgFromXml(activationSettingsElem, 2);
            Arg3 = GetArgFromXml(activationSettingsElem, 3);
            Arg4 = GetArgFromXml(activationSettingsElem, 4);
            Arg5 = GetArgFromXml(activationSettingsElem, 5);
            Arg6 = GetArgFromXml(activationSettingsElem, 6);
            Arg7 = GetArgFromXml(activationSettingsElem, 7);
            Arg8 = GetArgFromXml(activationSettingsElem, 8);
            Arg9 = GetArgFromXml(activationSettingsElem, 9);
            Arg10 = GetArgFromXml(activationSettingsElem, 10);
            return;
        }

        //Properties
        /// <summary>
        /// Returns working range interval of current activation function.
        /// </summary>
        public Interval WorkingRange
        {
            get
            {
                IActivationFunction af = AnalogActivationFactory.CreateActivationFunction(this);
                return af.Range;
            }
        }

        //Methods
        private double GetArgFromXml(XElement elem, int argNum)
        {
            double arg = double.NaN;
            string attrValue = elem.Attribute("arg" + argNum.ToString()).Value;
            if (attrValue != "NA")
            {
                arg = double.Parse(attrValue, CultureInfo.InvariantCulture);
            }
            return arg;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            AnalogActivationSettings cmpSettings = obj as AnalogActivationSettings;
            if (FunctionType != cmpSettings.FunctionType ||
                !Arg1.Equals(cmpSettings.Arg1) ||
                !Arg2.Equals(cmpSettings.Arg2) ||
                !Arg3.Equals(cmpSettings.Arg3) ||
                !Arg4.Equals(cmpSettings.Arg4) ||
                !Arg5.Equals(cmpSettings.Arg5) ||
                !Arg6.Equals(cmpSettings.Arg6) ||
                !Arg7.Equals(cmpSettings.Arg7) ||
                !Arg8.Equals(cmpSettings.Arg8) ||
                !Arg9.Equals(cmpSettings.Arg9) ||
                !Arg10.Equals(cmpSettings.Arg10)
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
        public AnalogActivationSettings DeepClone()
        {
            AnalogActivationSettings clone = new AnalogActivationSettings(this);
            return clone;
        }

    }//ActivationSettings

}//Namespace
