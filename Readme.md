# Reservoir Computing for .NET (RCNet)
The aim of the project is to make the [reservoir computing](https://en.wikipedia.org/wiki/Reservoir_computing) methods  easy to use and available for .net platform without dependency on external heterogeneous libraries.
Two main reservoir computing methods are called Echo State Network (ESN) and Liquid State Machine (LSM).
The implemented solution supports both of these methods. However, since ESN and LSM are based on very similar general principles, RCNet brings the option to combine them at the same time. It means the possibility to design complex "hybrid" recurrent networks with spiking and analog neurons working together. This approach, as I believe, opens up new interesting possibilities. This general implementation is called "**State Machine**" in the context of RCNet.
<br/>
Contact:<br/>
oldrich.kozelsky@email.cz

## Technical information
 - Source code is written in C# 6.0
 - Most components are serializable
 - Backward compatibility is not guaranteed
 - RCNet documentation is located on [wiki pages](https://github.com/okozelsk/NET/wiki)

## Demo application
Main RCNet functionality is demonstrated in a simple demo application (/Demo/DemoConsoleApp). Application has no startup parameters, all necessary settins are specified in DemoSettings.xml file. DemoSettings.xml has to be in the same folder as the executable DemoConsoleApp.exe. Application performs sequence of demo cases defined in DemoSettings.xml. The input data for each demo case must be stored in the csv file. You can modify DemoSettings.xml and configure your own tasks or modify and tune existing ones.


## Main implemented components
### Data handling
|Component|Description|
|--|--|
|BasicStat|Implements the simple and thread safe statistics|
|Normalizer|Data normalization/denormalization, Gaussian standardization|
|PatternBundle|Bundle of pattern and desired output vector|
|PatternDataLoader|The class allows to upload sample data for a Classification or Hybrid task from a csv file|
|TimeSeriesBundle|Bundle of input vector and desired output vector|
|TimeSeriesDataLoader|The class allows to upload sample data for a Prediction task from a csv file|
|BundleNormalizer|Helper class for easy standardization and normalization/naturalization of sample data bundle|

### Math
|Component|Description|
|--|--|
|ODENumSolver|Ordinary Differential Equations (ODE) Numerical Solver (Euler and RK4 methods)|
|Vector|Class represents the mathematical vector of double values|
|Matrix|Class represents the mathematical matrix of double values|
|EVD|Eigenvalues and eigenvectors of a real matrix|
|QRD|QR Decomposition|
|PhysUnit|Encaptualates SI physical unit|

### Analog neuron activation functions
|Component|Description|
|--|--|
|BentIdentity|Bent identity activation function|
|Elliot|Elliot activation function (aka Softsign)|
|Gaussian|Gaussian activation function|
|Identity|Identity activation function (aka Linear)|
|ISRU|ISRU (Inverse Square Root Unit) activation function|
|LeakyReLU|Leaky ReLU (Leaky Rectified Linear Unit) activation function|
|Sigmoid|Sigmoid activation function|
|Sinc|Sinc activation function|
|Sinusoid|Sinusoid activation function|
|SoftExponential|Soft exponential activation function|
|SoftPlus|Soft Plus activation function|
|TanH|TanH activation function|
See the [wiki pages.](https://en.wikipedia.org/wiki/Activation_function)

### Spiking neuron activation functions
|Component|Description|
|--|--|
|SimpleIF|Simple Integrate and Fire activation function|
|AdSimpleIF|Adaptive Simple Integrate and Fire activation function|
|LeakyIF|Leaky Integrate and Fire activation function|
|ExpIF|Exponential Integrate and Fire activation function|
|AdExpIF|Adaptive Exponential Integrate and Fire activation function|
See the [wiki pages.](https://en.wikipedia.org/wiki/Biological_neuron_model)

### Non-recurrent Networks
|Component|Description|
|--|--|
|FeedForwardNetwork|Implements the feed forward network supporting multiple hidden layers|
|LinRegrTrainer|Implements the linear regression trainer of the feed forward network. This is the special case trainer for FF network having no hidden layers and Identity output activation function|
|RPropTrainer|Resilient propagation trainer (iRPROP+ variant) of the feed forward network|
|ParallelPerceptron|Implements the parallel perceptron network|
|PDeltaRuleTrainer|P-Delta rule trainer of the parallel perceptron network|

### State Machine Components
|Component|Description|
|--|--|
|StaticSynapse|Static synapse computes constantly weighted signal from source to target neuron|
|InputAnalogNeuron|Input neuron is the special type of very simple neuron. Its purpose is only to mediate input analog value for a synapse|
|InputSpikingNeuron|Spiking input neuron is the special type of neuron. Its purpose is to preprocess input analog value to be deliverable as the spike train signal into the reservoir neurons through a synapse|
|ReservoirAnalogNeuron|Reservoir neuron is the main type of the neuron processing input stimuli and emitting output signal. Analog neuron produces analog output. Main features: Retainment (leaky integrator), The second power as augmented readout state|
|ReservoirSpikingNeuron|Reservoir neuron is the main type of the neuron processing input stimuli and emitting output signal. Spiking neuron produces spikes. Main features: Firing rate as primary readout state, Membrane potential as augmented readout state|
|Reservoir|Implements recurrent network supporting analog and spiking neurons working together. Main features: SpectralRadius, Multiple 3D pools of neurons, Pool to pool connections. It can work as the Echo State Network reservoir, Liquid State Machine reservoir or Mixed reservoir|
|ReadoutUnit|Contains the trained unit associated with output field and related important error statistics. Trained unit can be the Feed Forward Network or the Parallel Perceptron Network|
|ReadoutLayer|Class implements the common readout layer for the reservoir computing methods. Supports x-fold cross validation method.|
|StateMachine|Encaptulates the State Machine Network. Supports multiple internal recurrent reservoirs having multiple interconnected/cooperating analog and spiking neuron pools. Solves task types: Prediction, Classification, Hybrid|

