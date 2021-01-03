namespace RCNet.MathTools
{
    /// <summary>
    /// Implements the SI units and quantity prefixes.
    /// </summary>
    public static class PhysUnit
    {
        /// <summary>
        /// SI unit (base or derived).
        /// </summary>
        public enum SIUnit
        {
            /// <summary>
            /// The unit of the length.
            /// </summary>
            Metre,
            /// <summary>
            /// The unit of the mass.
            /// </summary>
            Kilogram,
            /// <summary>
            /// The unit of the time.
            /// </summary>
            Second,
            /// <summary>
            /// The unit of the electric current.
            /// </summary>
            Ampere,
            /// <summary>
            /// The unit of the thermodynamic temperature.
            /// </summary>
            Kelvin,
            /// <summary>
            /// The unit of the amount of substance.
            /// </summary>
            Mole,
            /// <summary>
            /// The unit of the luminous intensity.
            /// </summary>
            Candela,
            /// <summary>
            /// The unit of the angle.
            /// </summary>
            Radian,
            /// <summary>
            /// The unit of the solid angle.
            /// </summary>
            Steradian,
            /// <summary>
            /// The unit of the frequency.
            /// </summary>
            Hertz,
            /// <summary>
            /// The unit of the force.
            /// </summary>
            Newton,
            /// <summary>
            /// The unit of the pressure, stress.
            /// </summary>
            Pascal,
            /// <summary>
            /// The unit of the energy, work, heat.
            /// </summary>
            Joule,
            /// <summary>
            /// The unit of the power, radiant flux.
            /// </summary>
            Watt,
            /// <summary>
            /// The unit of the electric charge, quantity of electricity.
            /// </summary>
            Coulomb,
            /// <summary>
            /// The unit of the voltage (electrical potential).
            /// </summary>
            Volt,
            /// <summary>
            /// The unit of the capacitance.
            /// </summary>
            Farad,
            /// <summary>
            /// The unit of the resistance, impedance, reactance.
            /// </summary>
            Ohm,
            /// <summary>
            /// The unit of the conductance.
            /// </summary>
            Siemens,
            /// <summary>
            /// The unit of the magnetic flux.
            /// </summary>
            Weber,
            /// <summary>
            /// The unit of the magnetic flux density.
            /// </summary>
            Tesla,
            /// <summary>
            /// The unit of the inductance.
            /// </summary>
            Henry,
            /// <summary>
            /// The unit of the temperature relative to 273.15 Kelvin.
            /// </summary>
            Celsius,
            /// <summary>
            /// The unit of the luminous flux.
            /// </summary>
            Lumen,
            /// <summary>
            /// The unit of the illuminance.
            /// </summary>
            Lux,
            /// <summary>
            /// The unit of the radioactivity.
            /// </summary>
            Becquerel,
            /// <summary>
            /// The unit of the absorbed dose.
            /// </summary>
            Gray,
            /// <summary>
            /// The unit of the equivalent dose.
            /// </summary>
            Sievert,
            /// <summary>
            /// The unit of the catalytic activity.
            /// </summary>
            Katal
        }

        /// <summary>
        /// SI unit prefix
        /// </summary>
        public enum UnitPrefix
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
        /// Converts the specified quantity to the base quantity.
        /// </summary>
        /// <param name="quantity">The quantity to be converted.</param>
        /// <param name="prefix">The corresponding prefix of the specified quantity.</param>
        public static double ToBase(double quantity, UnitPrefix prefix)
        {
            switch (prefix)
            {
                case UnitPrefix.None: return quantity;
                case UnitPrefix.Atto: return quantity * 1e-18;
                case UnitPrefix.Femto: return quantity * 1e-15;
                case UnitPrefix.Piko: return quantity * 1e-12;
                case UnitPrefix.Nano: return quantity * 1e-9;
                case UnitPrefix.Mikro: return quantity * 1e-6;
                case UnitPrefix.Milli: return quantity * 1e-3;
                case UnitPrefix.Kilo: return quantity * 1e3;
                case UnitPrefix.Mega: return quantity * 1e6;
                case UnitPrefix.Giga: return quantity * 1e9;
                case UnitPrefix.Tera: return quantity * 1e12;
                case UnitPrefix.Peta: return quantity * 1e15;
                case UnitPrefix.Exa: return quantity * 1e18;
                default: return quantity;
            }
        }

        /// <summary>
        /// Converts the base quantity to specified quantity.
        /// </summary>
        /// <param name="quantity">The base quantity.</param>
        /// <param name="prefix">The required quantity prefix.</param>
        public static double FromBase(double quantity, UnitPrefix prefix)
        {
            switch (prefix)
            {
                case UnitPrefix.None: return quantity;
                case UnitPrefix.Atto: return quantity * 1e18;
                case UnitPrefix.Femto: return quantity * 1e15;
                case UnitPrefix.Piko: return quantity * 1e12;
                case UnitPrefix.Nano: return quantity * 1e9;
                case UnitPrefix.Mikro: return quantity * 1e6;
                case UnitPrefix.Milli: return quantity * 1e3;
                case UnitPrefix.Kilo: return quantity * 1e-3;
                case UnitPrefix.Mega: return quantity * 1e-6;
                case UnitPrefix.Giga: return quantity * 1e-9;
                case UnitPrefix.Tera: return quantity * 1e-12;
                case UnitPrefix.Peta: return quantity * 1e-15;
                case UnitPrefix.Exa: return quantity * 1e-18;
                default: return quantity;
            }
        }

    }//PhysUnit

}//Namespace
