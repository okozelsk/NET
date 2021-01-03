namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Implements the efficacy computer having the constant dynamics.
    /// </summary>
    public class ConstantEfficacy : IEfficacy
    {
        //Attributes
        private readonly ConstantDynamicsSettings _dynamicsCfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="dynamicsCfg">The configuration of the dynamics.</param>
        public ConstantEfficacy(ConstantDynamicsSettings dynamicsCfg)
        {
            _dynamicsCfg = dynamicsCfg;
            return;
        }

        //Methods
        /// <inheritdoc />
        public void Reset()
        {
            return;
        }

        /// <inheritdoc />
        public double Compute()
        {
            return _dynamicsCfg == null ? 1d : _dynamicsCfg.Efficacy;
        }

    }//ConstantEfficacy

}//Namespace
