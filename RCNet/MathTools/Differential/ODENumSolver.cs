using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools.VectorMath;

namespace RCNet.MathTools.Differential
{
    /// <summary>
    /// Implements simple numerical solver of Ordinary Differential Equation(s) 
    /// </summary>
    public static class ODENumSolver
    {
        //Constants
        /// <summary>
        /// Default number of computation sub-steps within the time step 
        /// </summary>
        public const int DefaultTimeSubSteps = 10;

        /// <summary>
        /// Supported solving methods
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
        /// Delegate of unknown ordinary differential equation (or couple of equations)
        /// </summary>
        /// <param name="t">Time</param>
        /// <param name="v">Vector of evolving values</param>
        /// <returns>dv/dt</returns>
        public delegate Vector Eqs(double t, Vector v);

        /// <summary>
        /// ODE solver function
        /// </summary>
        /// <param name="eqs">Ordinary differential equation or couple of differential equations</param>
        /// <param name="t0">Known start time</param>
        /// <param name="v0">Vector of known value(s) at start time</param>
        /// <param name="t">Target time</param>
        /// <param name="subSteps">Number of computation time-sub-steps</param>
        /// <param name="method">Solution method to be used</param>
        /// <returns>Solution estimations at computation time sub-steps</returns>
        public static IEnumerable<Estimation> Solve(Eqs eqs, double t0, Vector v0, double t, int subSteps = DefaultTimeSubSteps, Method method = Method.Euler)
        {
            double h = (t - t0) / subSteps;
            double currT = t0;
            Vector estimV = v0.Clone();
            for (int step = 0; step < subSteps; step++)
            {
                switch(method)
                {
                    case Method.Euler:
                        estimV = EulerSubStep(eqs, currT, estimV, h);
                        break;
                    case Method.RK4:
                        estimV = RK4SubStep(eqs, currT, estimV, h);
                        break;
                }
                currT += h;
                yield return new Estimation(currT, new Vector(estimV));
            }
            yield break;
        }

        //Euler 1st order computation sub-step
        private static Vector EulerSubStep(Eqs eqs, double t0, Vector v0, double h)
        {
            return v0 + h * eqs(t0, v0);
        }

        //Runge-Kutta 4th order computation sub-step
        private static Vector RK4SubStep(Eqs eqs, double t0, Vector v0, double h)
        {
            Vector k1 = h * eqs(t0, v0);
            Vector k2 = h * eqs(t0 + h / 2, v0 + k1 / 2);
            Vector k3 = h * eqs(t0 + h / 2, v0 + k2 / 2);
            Vector k4 = h * eqs(t0 + h, v0 + k3);
            return v0 + (k1 + 2*k2 + 2*k3 + k4) / 6d;
        }

        //Inner classes
        /// <summary>
        /// Represents solution estimation in time T
        /// </summary>
        [Serializable]
        public class Estimation
        {
            /// <summary>
            /// Time
            /// </summary>
            public double T { get; }
            /// <summary>
            /// Estimated value(s)
            /// </summary>
            public Vector V { get; }
            
            /// <summary>
            /// Instantiates an initialized instance
            /// </summary>
            /// <param name="t"></param>
            /// <param name="v"></param>
            public Estimation(double t, Vector v)
            {
                T = t;
                V = v;
                return;
            }

        }//Estimation

    }//ODENumSolver

}//Namespace
