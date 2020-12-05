namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Implements the constant efficacy computer
    /// </summary>
    public class ConstantEfficacy : IEfficacy
    {
        //Attributes
        private readonly ConstantDynamicsSettings _dynamicsCfg;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="dynamicsCfg">Dynamics configuration</param>
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
