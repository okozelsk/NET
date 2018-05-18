using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.MathTools
{
    /// <summary>
    /// Encaptualates SI physical unit
    /// </summary>
    public static class PhysUnit
    {
        /// <summary>
        /// SI base and derived units
        /// </summary>
        public enum SIUnit
        {
            /// <summary>
            /// Length
            /// </summary>
            Metre,
            /// <summary>
            /// Mass
            /// </summary>
            Kilogram,
            /// <summary>
            /// Time
            /// </summary>
            Second,
            /// <summary>
            /// Electric current
            /// </summary>
            Ampere,
            /// <summary>
            /// Thermodynamic temperature
            /// </summary>
            Kelvin,
            /// <summary>
            /// Amount of substance
            /// </summary>
            Mole,
            /// <summary>
            /// Luminous intensity
            /// </summary>
            Candela,
            /// <summary>
            /// Angle
            /// </summary>
            Radian,
            /// <summary>
            /// Solid angle
            /// </summary>
            Steradian,
            /// <summary>
            /// Frequency
            /// </summary>
            Hertz,
            /// <summary>
            /// Force
            /// </summary>
            Newton,
            /// <summary>
            /// Pressure, stress
            /// </summary>
            Pascal,
            /// <summary>
            /// Energy, work, heat
            /// </summary>
            Joule,
            /// <summary>
            /// Power, radiant flux
            /// </summary>
            Watt,
            /// <summary>
            /// Electric charge, quantity of electricity
            /// </summary>
            Coulomb,
            /// <summary>
            /// Voltage (electrical potential)
            /// </summary>
            Volt,
            /// <summary>
            /// Capacitance
            /// </summary>
            Farad,
            /// <summary>
            /// Resistance, impedance, reactance
            /// </summary>
            Ohm,
            /// <summary>
            /// Conductance
            /// </summary>
            Siemens,
            /// <summary>
            /// Magnetic flux
            /// </summary>
            Weber,
            /// <summary>
            /// Magnetic flux density
            /// </summary>
            Tesla,
            /// <summary>
            /// Inductance
            /// </summary>
            Henry,
            /// <summary>
            /// Temperature relative to 273.15 Kelvin
            /// </summary>
            Celsius,
            /// <summary>
            /// Luminous flux
            /// </summary>
            Lumen,
            /// <summary>
            /// Illuminance
            /// </summary>
            Lux,
            /// <summary>
            /// Radioactivity
            /// </summary>
            Becquerel,
            /// <summary>
            /// Absorbed dose
            /// </summary>
            Gray,
            /// <summary>
            /// Equivalent dose
            /// </summary>
            Sievert,
            /// <summary>
            /// Catalytic activity
            /// </summary>
            Katal
        }

        /// <summary>
        /// Metric prefixes
        /// </summary>
        public enum MetricPrefix
        {
            /// <summary>
            /// Base
            /// </summary>
            None,
            /// <summary>
            /// 1e-18
            /// </summary>
            Atto,
            /// <summary>
            /// 1e-15
            /// </summary>
            Femto,
            /// <summary>
            /// 1e-12
            /// </summary>
            Piko,
            /// <summary>
            /// 1e-9
            /// </summary>
            Nano,
            /// <summary>
            /// 1e-6
            /// </summary>
            Mikro,
            /// <summary>
            /// 1e-3
            /// </summary>
            Milli,
            /// <summary>
            /// 1e3
            /// </summary>
            Kilo,
            /// <summary>
            /// 1e6
            /// </summary>
            Mega,
            /// <summary>
            /// 1e9
            /// </summary>
            Giga,
            /// <summary>
            /// 1e12
            /// </summary>
            Tera,
            /// <summary>
            /// 1e15
            /// </summary>
            Peta,
            /// <summary>
            /// 1e18
            /// </summary>
            Exa
        }

        /// <summary>
        /// Conversion to basic unit quantity
        /// </summary>
        /// <param name="quantity">Quantity</param>
        /// <param name="prefix">Metric prefix corresponding to specified quantity</param>
        public static double ToBase(double quantity, MetricPrefix prefix)
        {
            switch(prefix)
            {
                case MetricPrefix.None: return quantity;
                case MetricPrefix.Atto: return quantity * 1e-18;
                case MetricPrefix.Femto: return quantity * 1e-15;
                case MetricPrefix.Piko: return quantity * 1e-12;
                case MetricPrefix.Nano: return quantity * 1e-9;
                case MetricPrefix.Mikro: return quantity * 1e-6;
                case MetricPrefix.Milli: return quantity * 1e-3;
                case MetricPrefix.Kilo: return quantity * 1e3;
                case MetricPrefix.Mega: return quantity * 1e6;
                case MetricPrefix.Giga: return quantity * 1e9;
                case MetricPrefix.Tera: return quantity * 1e12;
                case MetricPrefix.Peta: return quantity * 1e15;
                case MetricPrefix.Exa: return quantity * 1e18;
                default: return quantity;
            }
        }

        /// <summary>
        /// Conversion from basic unit quantity to a multiple or fraction
        /// </summary>
        /// <param name="quantity">Basic unit quantity</param>
        /// <param name="prefix">Metric prefix corresponding to desired multiple or fraction</param>
        public static double FromBase(double quantity, MetricPrefix prefix)
        {
            switch (prefix)
            {
                case MetricPrefix.None: return quantity;
                case MetricPrefix.Atto: return quantity * 1e18;
                case MetricPrefix.Femto: return quantity * 1e15;
                case MetricPrefix.Piko: return quantity * 1e12;
                case MetricPrefix.Nano: return quantity * 1e9;
                case MetricPrefix.Mikro: return quantity * 1e6;
                case MetricPrefix.Milli: return quantity * 1e3;
                case MetricPrefix.Kilo: return quantity * 1e-3;
                case MetricPrefix.Mega: return quantity * 1e-6;
                case MetricPrefix.Giga: return quantity * 1e-9;
                case MetricPrefix.Tera: return quantity * 1e-12;
                case MetricPrefix.Peta: return quantity * 1e-15;
                case MetricPrefix.Exa: return quantity * 1e-18;
                default: return quantity;
            }
        }

        /// <summary>
        /// Represents quantity in physical unit
        /// </summary>
        [Serializable]
        public class Quantity
        {
            //Attributes
            private SIUnit _unit;
            private double _quantity;

            //Constructor
            /// <summary>
            /// Instantiates an uninitialized instance
            /// </summary>
            /// <param name="unit">SI unit</param>
            public Quantity(SIUnit unit)
            {
                _unit = unit;
                _quantity = 0;
                return;
            }

            /// <summary>
            /// Instantiates an initialized instance
            /// </summary>
            /// <param name="quantity">Quantity</param>
            /// <param name="prefix">Metric prefix corresponding to specified quantity</param>
            /// <param name="unit">Unit of specified quantity</param>
            public Quantity(double quantity, MetricPrefix prefix, SIUnit unit)
            {
                _unit = unit;
                _quantity = ToBase(quantity, prefix);
                return;
            }

            //Properties
            public SIUnit BasicUnit { get { return _unit; } }
            public double BasicUnitQuantity { get { return _quantity; } }

            //Methods
            /// <summary>
            /// Returns multiple or fraction of the quantity
            /// </summary>
            /// <param name="prefix">Metric prefix corresponding to desired multiple or fraction of the basic unit</param>
            public double Get(MetricPrefix prefix = MetricPrefix.None)
            {
                return FromBase(_quantity, prefix);
            }

            /// <summary>
            /// Sets new quantity
            /// </summary>
            /// <param name="quantity">Quantity</param>
            /// <param name="prefix">Metric prefix corresponding to specified quantity</param>
            public void Set(double quantity, MetricPrefix prefix = MetricPrefix.None)
            {
                _quantity = ToBase(quantity, prefix);
                return;
            }

        }//Quantity

    }//PhysUnit
}//Namespace
