# Reservoir Computing for .NET (RCNet)
![Reservoir Computing conceptual view](https://github.com/okozelsk/NET/blob/master/RCNet/Docs/Imgs/ReservoirComputing_BasicSchema.png)
<br>
The aim of this project is to make the [reservoir computing](https://en.wikipedia.org/wiki/Reservoir_computing) methods  easy to use and available for .net platform without any other dependencies.
Two main reservoir computing methods are called Echo State Network (ESN) and Liquid State Machine (LSM).
RCNet supports both of these methods. Moreover, since ESN and LSM are based on similar general principles, RCNet allows to design complex "hybrid" recurrent reservoirs consisting of spiking and analog neurons synaptically linked together.
Mutual cooperation of hidden neurons having stateless analog and spiking activation functions is enabled by specific implementation of hidden neuron. Hidden neuron is not stateless and it can fire spikes even in case of stateless analog activation is used. "Anaolog spikes" are based on defined firing event depending on current and previous values of activation.
Hidden neuron also provides a standardized set of predictors no matter what activation function is used. According to preliminary results, it seems that it is no longer true that ESN is not capable to generalize and separate input signal enaugh to perform excellent classification. On the contrary. It now appears that the use of pure analog activations (like TanH) and the simple classical ESN reservoir design could be an "unexpected" competitor to spiking LSM reservoirs.
<br/>
The main component of RCNet is called "**State Machine**" and it has to be instantiated through xml configuration. "**State Machine**" is serializable so it is easily possible to instantiate and train it and than use it as a real-time loadable component in the solution.
<br/>
Source code is written in C# 7.3 (.NET framework 4.7.2).
More detailed documentation will be posted [here](https://github.com/okozelsk/NET/wiki) as soon as the current stage of the wild changes is over.
<br/>
I welcome questions, ideas and suggestions for improvements, usage experiences, bug alerts, constructive comments, etc... Please use my email address oldrich.kozelsky@email.cz to contact me.


## Demo application
Main functionality and possibilities are demonstrated in a simple [demo application](https://github.com/okozelsk/NET/tree/master/Demo/DemoConsoleApp). Application has no startup parameters, all necessary settins are specified in [DemoSettings.xml](https://github.com/okozelsk/NET/blob/master/Demo/DemoConsoleApp/DemoSettings.xml) file. DemoSettings.xml has to be in the same folder as the executable DemoConsoleApp.exe. Application performs sequence of tasks defined in DemoSettings.xml. You can easily insert new task or tune existing one by simple modification of DemoSettings.xml. Each task is defined in the xml element "case".
<br>
DemoSettings.xml contains several classification problems from the:
[Anthony Bagnall, Jason Lines, William Vickers and Eamonn Keogh, The UEA & UCR Time Series Classification Repository, www.timeseriesclassification.com](https://timeseriesclassification.com)
site. It is shown, that even without any massive tuning of StateMachine's parameters, it exhibits very similar or better classification accuracy as the best algorithms referenced on the site.


### Data format for the demo application
Input data is expected in csv format where data delimiter can be a tab, semicolon or comma character.
* **Continuous feeding regime** requires a standard csv format, where the first line contains the names of the data fields and each next line contains the data. [Here](https://github.com/okozelsk/NET/blob/master/Demo/DemoConsoleApp/Data/TTOO.csv) is an example
* **Patterned feeding regime** requires specific logical csv format without colum names (header). Each data line contains values of repetitive pattern features followed by expected output values at the end. Values of repetitive pattern features can be organized in two ways: groupped [v1(t1),v2(t1),v1(t2),v2(t2),v1(t3),v2(t3)] or sequential [v1(t1),v1(t2),v1(t3),v2(t1),v2(t2),v2(t3)]. [Here](https://github.com/okozelsk/NET/blob/master/Demo/DemoConsoleApp/Data/LibrasMovement.csv) is an example

## Components overview

![Reservoir Computing conceptual view](https://github.com/okozelsk/NET/blob/master/RCNet/Docs/Imgs/StateMachine_EntityRelationship.png)
<br/>
(listed in logical order from basic to composite and complex)

### Math
|Component|Description|
|--|--|
|[BasicStat](https://github.com/okozelsk/NET/blob/master/RCNet/MathTools/BasicStat.cs)|Provides basic statistics of given data (averages, sum of squares, standard deviation, etc.)|
|[WeightedAvg](https://github.com/okozelsk/NET/blob/master/RCNet/MathTools/WeightedAvg.cs)|Computes weighted average of given value/weight data pairs|
|[MovingDataWindow](https://github.com/okozelsk/NET/blob/master/RCNet/MathTools/MovingDataWindow.cs)|Implements moving data window and offers computation of weighted average of recent part of given data|
|[ODENumSolver](https://github.com/okozelsk/NET/blob/master/RCNet/MathTools/Differential/ODENumSolver.cs)|Implements ordinary differential equations (ODE) numerical solver supporting Euler and RK4 methods|
|[Vector](https://github.com/okozelsk/NET/blob/master/RCNet/MathTools/VectorMath/Vector.cs)|Implements vector of double values supporting basic mathematical operations|
|[Matrix](https://github.com/okozelsk/NET/blob/master/RCNet/MathTools/MatrixMath/Matrix.cs)|Implements matrix of double values supporting basic mathematical operations. Contains buit-in Power Iteration method for the largest eigen value quick estimation|
|[EVD](https://github.com/okozelsk/NET/blob/master/RCNet/MathTools/MatrixMath/EVD.cs)|Full eigen values and vectors decomposition of a squared matrix|
|[SVD](https://github.com/okozelsk/NET/blob/master/RCNet/MathTools/MatrixMath/SVD.cs)|Singular values decomposition of a matrix|
|[QRD](https://github.com/okozelsk/NET/blob/master/RCNet/MathTools/MatrixMath/QRD.cs)|QR decomposition of a matrix|
|[LUD](https://github.com/okozelsk/NET/blob/master/RCNet/MathTools/MatrixMath/LUD.cs)|LU decomposition of a squared matrix|
|[ParamSeeker](https://github.com/okozelsk/NET/blob/master/RCNet/MathTools/PS/ParamSeeker.cs)|Implements an error driven iterative search for the best value of a given parameter|
|[HurstExpEstim](https://github.com/okozelsk/NET/blob/master/RCNet/MathTools/Hurst/HurstExpEstim.cs)|Implements Hurst exponent estimator and Rescalled range. It can be used to evaluate level of data randomness|
|["RandomValue"](https://github.com/okozelsk/NET/tree/master/RCNet/RandomValue)|Supports Uniform, Gaussian, Exponential and Gamma distributions. Here is [extension code](https://github.com/okozelsk/NET/blob/master/RCNet/Extensions/RandomExtensions.cs)|
|[Others](https://github.com/okozelsk/NET/tree/master/RCNet/MathTools)|Set of small additional helper components like PhysUnit, Interval, Bitwise, Combinatorics, Discrete,...|

### Signal generators
|Component|Description|
|--|--|
|[PulseGenerator](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Data/Generators/PulseGenerator.cs)|Generates constant pulses having specified average period. Pulse leaks follow specified random distribution or can be constant|
|[MackeyGlassGenerator](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Data/Generators/MackeyGlassGenerator.cs)|Generates Mackey-Glass chaotic signal|
|[RandomGenerator](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Data/Generators/RandomGenerator.cs)|Generates random signal following specified distribution|
|[SinusoidalGenerator](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Data/Generators/SinusoidalGenerator.cs)|Generates sinusoidal signal|

### XML handling
|Component|Description|
|--|--|
|[ElemValidator](https://github.com/okozelsk/NET/blob/master/RCNet/XmlTools/ElemValidator.cs)|Implements validation of alone xml element against specified type defined in xsd. It complements the apparently missing .net method and is necessary to comply with the overall RCNet xml settings concept.|
|[DocValidator](https://github.com/okozelsk/NET/blob/master/RCNet/XmlTools/DocValidator.cs)|Envelopes xml document loading and validation|

### Data handling
|Component|Description|
|--|--|
|[SimpleQueue](https://github.com/okozelsk/NET/blob/master/RCNet/Queue/SimpleQueue.cs)|Implements quick and simple FIFO queue (template). Supports access to enqueued elements so it can be also used as the "sliding window"|
|[BinFeatureFilter](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Data/Filter/BinFeatureFilter.cs)|Binary (0/1) feature filter|
|[EnumFeatureFilter](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Data/Filter/EnumFeatureFIlter.cs)|Enumeration (1..N) feature filter|
|[RealFeatureFilter](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Data/Filter/RealFeatureFilter.cs)|Real number feature filter supporting standardization and range reserve for handling of unseen data in the future|
|[DelimitedStringValues](https://github.com/okozelsk/NET/blob/master/RCNet/CsvTools/DelimitedStringValues.cs)|Helper encoder and decoder of data line in csv format|
|[CsvDataHolder](https://github.com/okozelsk/NET/blob/master/RCNet/CsvTools/CsvDataHolder.cs)|Provides simple loading and saving of csv data|
|[VectorBundle](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Data/VectorBundle.cs)|Bundle of input data vectors and corresponding desired output vectors (1:1). Supports upload from csv file|
|[InputPattern](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Data/InputPattern.cs)|Input pattern supporting signal detection, unification and resampling features|
|[ResultBundle](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Data/ResultBundle.cs)|Bundle of input, computed and desired output vectors (1:1:1)|

### Analog activation functions (stateless)
See the [wiki pages.](https://en.wikipedia.org/wiki/Activation_function)

|Component|Description|
|--|--|
|[BentIdentity](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/BentIdentity.cs)|Bent identity activation function|
|[SQNL](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/SQNL.cs)|Square nonlinearity activation function|
|[Elliot](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/Elliot.cs)|Elliot activation function (aka Softsign)|
|[Gaussian](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/Gaussian.cs)|Gaussian activation function|
|[Identity](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/Identity.cs)|Identity activation function (aka Linear)|
|[ISRU](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/ISRU.cs)|ISRU (Inverse Square Root Unit) activation function|
|[LeakyReLU](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/LeakyReLU.cs)|Leaky ReLU (Leaky Rectified Linear Unit) activation function|
|[Sigmoid](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/Sinusoid.cs)|Sigmoid activation function|
|[Sinc](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/Sinc.cs)|Sinc activation function|
|[Sinusoid](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/Sinusoid.cs)|Sinusoid activation function|
|[SoftExponential](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/SoftExponential.cs)|Soft exponential activation function|
|[SoftPlus](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/SoftPlus.cs)|Soft Plus activation function|
|[TanH](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/TanH.cs)|TanH activation function|

### Spiking activation functions (stateful)
See the [wiki pages.](https://en.wikipedia.org/wiki/Biological_neuron_model)

|Component|Description|
|--|--|
|[SimpleIF](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/SimpleIF.cs)|Simple Integrate and Fire activation function|
|[LeakyIF](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/LeakyIF.cs)|Leaky Integrate and Fire activation function|
|[ExpIF](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/ExpIF.cs)|Exponential Integrate and Fire activation function|
|[AdExpIF](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/AdExpIF.cs)|Adaptive Exponential Integrate and Fire activation function|
|[IzhikevichIF](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Activation/IzhikevichIF.cs)|Izhikevich Integrate and Fire activation function (model "one fits all")|

### Non-recurrent networks and trainers
|Component|Description|
|--|--|
|[FeedForwardNetwork](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/NonRecurrent/FF/FeedForwardNetwork.cs)|Implements the feed forward network supporting multiple hidden layers|
|[RPropTrainer](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/NonRecurrent/FF/RPropTrainer.cs)|Resilient propagation (iRPROP+) trainer of the feed forward network|
|[QRDRegrTrainer](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/NonRecurrent/FF/QRDRegrTrainer.cs)|Implements the linear regression (QR decomposition) trainer of the feed forward network. This is the special case trainer for FF network having no hidden layers and Identity output activation function|
|[RidgeRegrTrainer](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/NonRecurrent/FF/RidgeRegrTrainer.cs)|Implements the ridge linear regression trainer of the feed forward network. This is the special case trainer for FF network having no hidden layers and Identity output activation function|
|[ElasticRegrTrainer](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/NonRecurrent/FF/ElasticRegrTrainer.cs)|Implements the elastic net trainer of the feed forward network. This is the special case trainer for FF network having no hidden layers and Identity output activation function|
|||
|[ParallelPerceptron](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/NonRecurrent/PP/ParallelPerceptron.cs)|Implements the parallel perceptron network|
|[PDeltaRuleTrainer](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/NonRecurrent/PP/PDeltaRuleTrainer.cs)|P-Delta rule trainer of the parallel perceptron network|
|||
|[TrainedNetwork](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/NonRecurrent/TrainedNetwork.cs)|Encapsulates trained non-recurrent (Feed forward or Parallel perceptron) network and related error statistics.|
|[TrainedNetworkBuilder](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/NonRecurrent/TrainedNetworkBuilder.cs)|Builds single trained (Feed forward or Parallel perceptron) network. Performs training epochs and offers control to user to evaluate the network.|
|[TrainedNetworkCluster](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/NonRecurrent/TrainedNetworkCluster.cs)|Encapsulates set of trained non-recurrent networks (cluster of TrainedNetwork instances) and related error statistics. Offers weighted cluster prediction and also publics all inner members sub-predictions.|
|[TrainedNetworkClusterBuilder](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/NonRecurrent/TrainedNetworkClusterBuilder.cs)|Builds cluster of trained networks based on x-fold cross validation approach. Each fold can have associated number of various networks.|

### State Machine components
|Component|Description|
|--|--|
|[InputSynapse](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/SM/Synapse/InputSynapse.cs)|Computes constantly weighted signal from external input and passes it to target neuron. Supports signal delay|
|[InternalSynapse](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/SM/Synapse/InternalSynapse.cs)|Computes dynamically weighted signal from source to target neuron. It supports pre-synaptic short-term plasticity.|
|[InputUnit](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/SM/Preprocessing/InputUnit.cs)|Provides external input for processing in the reservoir. Supports analog and spike-train codings (analog coding supports set of realtime transformations, spike-train coding method is from the "Sparse coding" family). Coded data is then further provided through set of InputNeurons.|
|[InputNeuron](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/SM/Neuron/InputNeuron.cs)|Input neuron is the special type of very simple neuron. Its purpose is only to mediate input signal for a synapse|
|[HiddenNeuron](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/SM/Neuron/HiddenNeuron.cs)|Supports both analog and spiking activation functions and can produce analog signal and/or spikes (neuron is able to fire spikes even when stateless analog activation is used). Supports Retainment property of analog activation (leaky integrator). Supports set of different predictors.|
|[Reservoir](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/SM/Preprocessing/Reservoir.cs)|Provides recurrent network supporting analog and spiking neurons working directly together. Main features: SpectralRadius (for weights of analog, spiking or both neurons), Multiple 3D pools of neurons, Pool to pool connections. It can work as the Echo State Network reservoir, Liquid State Machine reservoir or Mixed reservoir|
|[NeuralPreprocessor](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/SM/Preprocessing/NeuralPreprocessor.cs)|Provides data preprocessing to predictors. Supports multiple internal reservoirs. Supports virtual input data associated with predefined signal generators. Supports two input feeding regimes: Continuous and Patterned|
|[ReadoutUnit](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/SM/Readout/ReadoutUnit.cs)|Readout unit does the Forecast or Classification and encapsulates TrainedNetworkCluster.|
|[ReadoutLayer](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/SM/Readout/ReadoutLayer.cs)|Implements independent readout layer consisting of trained readout units.|

### State Machine
[StateMachine](https://github.com/okozelsk/NET/blob/master/RCNet/Neural/Network/SM/StateMachine.cs)
The main component. Encapsulates independent NeuralPreprocessor and ReadoutLayer components into the single component and adds support for routing specific predictors and input fields to the specific readout units. Allows to bypass NeuralPreprocessor and to use input data directly as a predictors for the readout layer.

### Xml setup recipe
[RCNetTypes.xsd](https://github.com/okozelsk/NET/blob/master/RCNet/RCNetTypes.xsd)
Defines all necessary types used in StateMachine's xml setup.
