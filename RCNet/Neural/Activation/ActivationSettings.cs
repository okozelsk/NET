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

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Class specifies properties of randomly generated values
    /// </summary>
    [Serializable]
    public class ActivationSettings
    {
        //Attribute properties
        /// <summary>
        /// Type of activation function
        /// </summary>
        public ActivationFactory.Function FunctionType { get; set; }
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
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg11 { get; set; }
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg12 { get; set; }
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg13 { get; set; }
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg14 { get; set; }
        /// <summary>
        /// Value of the argument to be passed to the activation function constructor
        /// </summary>
        public double Arg15 { get; set; }

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
        /// <param name="arg11">Value of the argument to be passed to the activation function constructor</param>
        /// <param name="arg12">Value of the argument to be passed to the activation function constructor</param>
        /// <param name="arg13">Value of the argument to be passed to the activation function constructor</param>
        /// <param name="arg14">Value of the argument to be passed to the activation function constructor</param>
        /// <param name="arg15">Value of the argument to be passed to the activation function constructor</param>
        public ActivationSettings(ActivationFactory.Function functionType,
                                        double arg1 = double.NaN,
                                        double arg2 = double.NaN,
                                        double arg3 = double.NaN,
                                        double arg4 = double.NaN,
                                        double arg5 = double.NaN,
                                        double arg6 = double.NaN,
                                        double arg7 = double.NaN,
                                        double arg8 = double.NaN,
                                        double arg9 = double.NaN,
                                        double arg10 = double.NaN,
                                        double arg11 = double.NaN,
                                        double arg12 = double.NaN,
                                        double arg13 = double.NaN,
                                        double arg14 = double.NaN,
                                        double arg15 = double.NaN
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
            Arg11 = arg11;
            Arg12 = arg12;
            Arg13 = arg13;
            Arg14 = arg14;
            Arg15 = arg15;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ActivationSettings(ActivationSettings source)
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
            Arg11 = source.Arg11;
            Arg12 = source.Arg12;
            Arg13 = source.Arg13;
            Arg14 = source.Arg14;
            Arg15 = source.Arg15;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing ActivationSettings settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ActivationSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Activation.ActivationSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement activationSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            FunctionType = ActivationFactory.ParseActivationFunctionType(activationSettingsElem.Attribute("function").Value);
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
            Arg11 = GetArgFromXml(activationSettingsElem, 11);
            Arg12 = GetArgFromXml(activationSettingsElem, 12);
            Arg13 = GetArgFromXml(activationSettingsElem, 13);
            Arg14 = GetArgFromXml(activationSettingsElem, 14);
            Arg15 = GetArgFromXml(activationSettingsElem, 15);
            return;
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
            ActivationSettings cmpSettings = obj as ActivationSettings;
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
                !Arg10.Equals(cmpSettings.Arg10) ||
                !Arg11.Equals(cmpSettings.Arg11) ||
                !Arg12.Equals(cmpSettings.Arg12) ||
                !Arg13.Equals(cmpSettings.Arg13) ||
                !Arg14.Equals(cmpSettings.Arg14) ||
                !Arg15.Equals(cmpSettings.Arg15)
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
        public ActivationSettings DeepClone()
        {
            ActivationSettings clone = new ActivationSettings(this);
            return clone;
        }

    }//ActivationSettings

}//Namespace
