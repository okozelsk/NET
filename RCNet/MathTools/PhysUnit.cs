using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.MathTools
{
    /// <summary>
    /// Encaptualates physical unit
    /// </summary>
    public static class PhysUnit
    {
        /// <summary>
        /// SI base and derived units
        /// </summary>
        public enum SI
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
        /// Physical unit quantity prefixes
        /// </summary>
        public enum QPrefix
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
        /// Conversion to base of physical unit
        /// </summary>
        /// <param name="v">Value</param>
        /// <param name="qPrefix">Unit quantity prefix</param>
        /// <returns>Base value</returns>
        public static double ToBase(double v, QPrefix qPrefix)
        {
            switch(qPrefix)
            {
                case QPrefix.None: return v;
                case QPrefix.Atto: return v * 1e-18;
                case QPrefix.Femto: return v * 1e-15;
                case QPrefix.Piko: return v * 1e-12;
                case QPrefix.Nano: return v * 1e-9;
                case QPrefix.Mikro: return v * 1e-6;
                case QPrefix.Milli: return v * 1e-3;
                case QPrefix.Kilo: return v * 1e3;
                case QPrefix.Mega: return v * 1e6;
                case QPrefix.Giga: return v * 1e9;
                case QPrefix.Tera: return v * 1e12;
                case QPrefix.Peta: return v * 1e15;
                case QPrefix.Exa: return v * 1e18;
                default: return v;
            }
        }

        /// <summary>
        /// Conversion from base of physical unit
        /// </summary>
        /// <param name="v">Value</param>
        /// <param name="qPrefix">Unit quantity prefix</param>
        public static double FromBase(double v, QPrefix qPrefix)
        {
            switch (qPrefix)
            {
                case QPrefix.None: return v;
                case QPrefix.Atto: return v * 1e18;
                case QPrefix.Femto: return v * 1e15;
                case QPrefix.Piko: return v * 1e12;
                case QPrefix.Nano: return v * 1e9;
                case QPrefix.Mikro: return v * 1e6;
                case QPrefix.Milli: return v * 1e3;
                case QPrefix.Kilo: return v * 1e-3;
                case QPrefix.Mega: return v * 1e-6;
                case QPrefix.Giga: return v * 1e-9;
                case QPrefix.Tera: return v * 1e-12;
                case QPrefix.Peta: return v * 1e-15;
                case QPrefix.Exa: return v * 1e-18;
                default: return v;
            }
        }

        /// <summary>
        /// Value in physical unit
        /// </summary>
        [Serializable]
        public class Value
        {
            //Attributes
            private SI _unit;
            private double _base;

            //Constructor
            /// <summary>
            /// Instantiates an uninitialized instance
            /// </summary>
            /// <param name="unit">Physical SI unit</param>
            public Value(SI unit)
            {
                _unit = unit;
                _base = 0;
                return;
            }

            /// <summary>
            /// Instantiates an initialized instance
            /// </summary>
            /// <param name="v">Initial value</param>
            /// <param name="qPrefix">Quantity prefix of v</param>
            /// <param name="unit">Physical SI unit of v</param>
            public Value(double v, QPrefix qPrefix, SI unit)
            {
                _unit = unit;
                _base = ToBase(v, qPrefix);
                return;
            }

            //Methods
            /// <summary>
            /// Returns converted base value
            /// </summary>
            /// <param name="qPrefix">Quantity prefix</param>
            public double Get(QPrefix qPrefix = QPrefix.None)
            {
                return FromBase(_base, qPrefix);
            }

            /// <summary>
            /// Sets new base value
            /// </summary>
            /// <param name="v">Value</param>
            /// <param name="qPrefix">Quantity prefix</param>
            public void Set(double v, QPrefix qPrefix)
            {
                _base = ToBase(v, qPrefix);
                return;
            }

            /// <summary>
            /// Adds given value to base value
            /// </summary>
            /// <param name="v">Value</param>
            /// <param name="qPrefix">Quantity prefix</param>
            public void Add(double v, QPrefix qPrefix)
            {
                _base += ToBase(v, qPrefix);
                return;
            }

        }//Value

    }//PhysUnit
}//Namespace
