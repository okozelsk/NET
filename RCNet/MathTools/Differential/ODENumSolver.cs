using RCNet.MathTools.VectorMath;
using System;
using System.Collections.Generic;

namespace RCNet.MathTools.Differential
{
    /// <summary>
    /// Implements a simple numerical solver of the Ordinary Differential Equation(s).
    /// </summary>
    public static class ODENumSolver
    {
        //Constants
        /// <summary>
        /// The default number of computation sub-steps within the time interval.
        /// </summary>
        public const int DefaultCompSubSteps = 10;

        /// <summary>
        /// The solving method.
        /// </summary>
        public enum Method
        {
            /// <summary>
            /// The Euler 1st order method.
            /// </summary>
            Euler,
            /// <summary>
            /// The Runge-Kutta 4th order method.
            /// </summary>
            RK4
        }

        /// <summary>
        /// Delegate of an unknown ordinary differential equation (or couple of equations).
        /// </summary>
        /// <param name="t">The time.</param>
        /// <param name="v">The vector of evolving values.</param>
        /// <returns>dv/dt</returns>
        public delegate Vector Eqs(double t, Vector v);

        /// <summary>
        /// Solves the ODE.
        /// </summary>
        /// <remarks>
        /// A gradual version.
        /// </remarks>
        /// <param name="eqs">An ordinary differential equation or couple of equations.</param>
        /// <param name="t0">The start time.</param>
        /// <param name="v0">The vector of known evolving value(s) at the start time.</param>
        /// <param name="t">The target time.</param>
        /// <param name="subSteps">The number of computation sub-steps within the time interval.</param>
        /// <param name="method">The numerical solving method to be used.</param>
        /// <returns>The solution estimations, gradually, at all computation sub-steps.</returns>
        public static IEnumerable<Estimation> SolveGradually(Eqs eqs, double t0, Vector v0, double t, int subSteps = DefaultCompSubSteps, Method method = Method.Euler)
        {
            double h = (t - t0) / subSteps;
            double currT = t0;
            Vector estimV = v0.Clone();
            for (int step = 0; step < subSteps; step++)
            {
                switch (method)
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

        /// <summary>
        /// Solves the ODE.
        /// </summary>
        /// <param name="eqs">An ordinary differential equation or couple of equations.</param>
        /// <param name="t0">The start time.</param>
        /// <param name="v0">The vector of known evolving value(s) at the start time.</param>
        /// <param name="t">The target time.</param>
        /// <param name="subSteps">The number of computation sub-steps within the time interval.</param>
        /// <param name="method">The numerical solving method to be used.</param>
        /// <returns>The solution estimation at the target time t.</returns>
        public static Vector Solve(Eqs eqs, double t0, Vector v0, double t, int subSteps = DefaultCompSubSteps, Method method = Method.Euler)
        {
            double h = (t - t0) / subSteps;
            double currT = t0;
            Vector estimV = v0.Clone();
            for (int step = 0; step < subSteps; step++)
            {
                switch (method)
                {
                    case Method.Euler:
                        estimV = EulerSubStep(eqs, currT, estimV, h);
                        break;
                    case Method.RK4:
                        estimV = RK4SubStep(eqs, currT, estimV, h);
                        break;
                }
                currT += h;
            }
            return estimV;
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
            return v0 + (k1 + 2 * k2 + 2 * k3 + k4) / 6d;
        }

        //Inner classes
        /// <summary>
        /// Holds the solution estimation in the timepoint T.
        /// </summary>
        [Serializable]
        public class Estimation
        {
            /// <summary>
            /// The time.
            /// </summary>
            public double T { get; }
            /// <summary>
            /// The vector of estimated values.
            /// </summary>
            public Vector V { get; }

            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="t">The time.</param>
            /// <param name="v">The vector of estimated values.</param>
            public Estimation(double t, Vector v)
            {
                T = t;
                V = v;
                return;
            }

        }//Estimation

    }//ODENumSolver

}//Namespace
