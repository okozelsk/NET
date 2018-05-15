using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.MathTools.Differential
{
    /// <summary>
    /// Autonomous Ordinary Differential Equation (dv/dt) numerical solver
    /// </summary>
    public static class AutonomousODE
    {
        //Constants
        /// <summary>
        /// Default number of time step computation sub-steps
        /// </summary>
        public const int DefaultSteps = 10;

        /// <summary>
        /// Solving methods
        /// </summary>
        public enum Method
        {
            /// <summary>
            /// Euler 1st order numerical method
            /// </summary>
            Euler,
            /// <summary>
            /// Runge-Kutta 4th order numerical method
            /// </summary>
            RK4
        }

        /// <summary>
        /// Delegate of unknown autonomous ordinary differential equation
        /// </summary>
        /// <param name="v">v</param>
        /// <returns>dv/dt</returns>
        public delegate double AutonomousEquation(double v);

        /// <summary>
        /// Atonomous ODE numerical solver
        /// </summary>
        /// <param name="eq">Autonomous ordinary diffferential equation</param>
        /// <param name="v">Current v(tCurrent)</param>
        /// <param name="t">Target time</param>
        /// <param name="steps">Number of computation steps dividing interval (t - t0)</param>
        /// <param name="method">Method to be used</param>
        /// <returns>New v approximation at target time t</returns>
        public static double Solve(AutonomousEquation eq, double v, double t, int steps = DefaultSteps, Method method = Method.Euler)
        {
            double h = t / steps;
            for(int step = 0; step < steps; step++)
            {
                switch(method)
                {
                    case Method.Euler:
                        v = EulerStep(eq, v, h);
                        break;
                    case Method.RK4:
                        v = RK4Step(eq, v, h);
                        break;
                }
            }
            return v;
        }

        //Euler 1st order step
        private static double EulerStep(AutonomousEquation eq, double v, double h)
        {
            return v + h * eq(v);
        }

        //Runge-Kutta 4th order step
        private static double RK4Step(AutonomousEquation eq, double v, double h)
        {
            double k1 = h * eq(v);
            double k2 = h * eq(v + k1 / 2);
            double k3 = h * eq(v + k2 / 2);
            double k4 = h * eq(v + k3);
            return v + (k1 + 2*k2 + 2*k3 + k4) / 6d;
        }



    }//AutonomousODE
}//Namespace
