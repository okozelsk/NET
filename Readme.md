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
Main functionality and possibilities are demonstrated in a simple demo application (/Demo/DemoConsoleApp). Application has no startup parameters, all necessary settins are specified in DemoSettings.xml file. DemoSettings.xml has to be in the same folder as the executable DemoConsoleApp.exe. Application performs sequence of tasks defined in DemoSettings.xml. You can easily insert new task or tune existing one by simple modification of DemoSettings.xml. Each task is defined in the xml element "case".
<br>
DemoSettings.xml contains several classification problems from the:
[Anthony Bagnall, Jason Lines, William Vickers and Eamonn Keogh, The UEA & UCR Time Series Classification Repository, www.timeseriesclassification.com](https://timeseriesclassification.com)
site. It is shown, that even without any massive tuning of StateMachine's parameters, it exhibits very similar or better classification accuracy as the best algorithms referenced on the site.


### Data format for the demo application
Input data is expected in csv format where delimiter can be a semicolon or a comma.
* **Continuous feeding regime** requires a standard csv format, where the first line contains the names of the data fields and each next line contains the numeric data. [Here](https://github.com/okozelsk/NET/blob/master/Demo/DemoConsoleApp/Data/TTOO.csv) is an example
* **Patterned feeding regime** requires specific csv format. First line specifies names of repetitive input features of the pattern (first must be keyword "#RepetitiveGroupOfAttributes"). Second line specifies names of output values (first must be keyword "#Outputs"). Each next line then contains Nx values of repetitive pattern features followed by expected output values at the end. [Here](https://github.com/okozelsk/NET/blob/master/Demo/DemoConsoleApp/Data/LibrasMovement.csv) is an example

## Components overview

![Reservoir Computing conceptual view](https://github.com/okozelsk/NET/blob/master/RCNet/Docs/Imgs/StateMachine_EntityRelationship.png)
<br/>
(listed in logical order from basic to composite and complex)

### Math
|Component|Description|
|--|--|
|BasicStat|Provides basic statistics of given data (averages, sum of squares, standard deviation, etc.)|
|WeightedAvg|Computes weighted average of given value/weight data pairs|
|MovingWeightedAvg|Computes weighted average of recent part of given data|
|ODENumSolver|Implements ordinary differential equations (ODE) numerical solver supporting Euler and RK4 methods|
|Vector|Implements vector of double values supporting basic mathematical operations|
|Matrix|Implements matrix of double values supporting basic mathematical operations. Contains buit-in Power Iteration method for the largest eigen value quick estimation|
|EVD|Full eigen values and vectors decomposition of a squared matrix|
|SVD|Singular values decomposition of a matrix|
|QRD|QR decomposition of a matrix|
|LUD|LU decomposition of a squared matrix|
|ParamSeeker|Implements an error driven iterative search for the best value of a given parameter|
|HurstExpEstim|Implements Hurst exponent estimator. It can be used to evaluate level of data randomness|
|"RandomValue"|Supports Uniform, Gaussian, Exponential and Gamma distributions|
|Others|Set of small additional helper components like PhysUnit, Interval, Bitwise, Combinatorics, Factorial,...|

### Signal generators
|Component|Description|
|--|--|
|PulseGenerator|Generates constant pulses having specified average period. Pulse leaks follow specified random distribution or can be constant|
|MackeyGlassGenerator|Generates Mackey-Glass chaotic signal|
|RandomGenerator|Generates random signal following specified distribution|
|SinusoidalGenerator|Generates sinusoidal signal|

### XML handling
|Component|Description|
|--|--|
|ElemValidator|Implements validation of alone xml element against specified type defined in xsd. It complements the apparently missing .net method and is necessary to comply with the overall RCNet xml settings concept.|
|DocValidator|Provides the xml document loading/validation functionalities|

### Data handling
|Component|Description|
|--|--|
|SimpleQueue|Implements quick and simple FIFO queue (template). Supports access to enqueued elements so it can be also used as the "sliding window"|
|BinFeatureFilter|Binary (0/1) feature filter|
|EnumFeatureFilter|Enumeration (1..N) feature filter|
|RealFeatureFilter|Real number feature filter supporting standardization and range reserve for handling of unseen data in the future|
|DelimitedStringValues|Helper encoder and decoder of data in csv format|
|VectorBundle|Bundle of input data vectors and corresponding desired output vectors (1:1). Supports upload from csv file|
|PatternBundle|Bundle of patterns of input data vectors and corresponding desired output vectors (n:1). Supports upload from csv file|
|ResultBundle|Bundle of input, computed and desired output vectors (1:1:1)|

### Analog activation functions (stateless)
See the [wiki pages.](https://en.wikipedia.org/wiki/Activation_function)

|Component|Description|
|--|--|
|BentIdentity|Bent identity activation function|
|SQNL|Square nonlinearity activation function|
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

### Spiking activation functions (stateful)
See the [wiki pages.](https://en.wikipedia.org/wiki/Biological_neuron_model)

|Component|Description|
|--|--|
|SimpleIF|Simple Integrate and Fire activation function|
|LeakyIF|Leaky Integrate and Fire activation function|
|ExpIF|Exponential Integrate and Fire activation function|
|AdExpIF|Adaptive Exponential Integrate and Fire activation function|
|IzhikevichIF|Izhikevich Integrate and Fire activation function (model "one fits all")|

### Non-recurrent networks and trainers
|Component|Description|
|--|--|
|FeedForwardNetwork|Implements the feed forward network supporting multiple hidden layers|
|RPropTrainer|Resilient propagation (iRPROP+) trainer of the feed forward network|
|QRDRegrTrainer|Implements the linear regression (QR decomposition) trainer of the feed forward network. This is the special case trainer for FF network having no hidden layers and Identity output activation function|
|RidgeRegrTrainer|Implements the ridge linear regression trainer of the feed forward network. This is the special case trainer for FF network having no hidden layers and Identity output activation function|
|ElasticRegrTrainer|Implements the elastic net trainer of the feed forward network. This is the special case trainer for FF network having no hidden layers and Identity output activation function|
|||
|ParallelPerceptron|Implements the parallel perceptron network|
|PDeltaRuleTrainer|P-Delta rule trainer of the parallel perceptron network|

### State Machine components
|Component|Description|
|--|--|
|InputSynapse|Computes constantly weighted signal from external input and passes it to target neuron. Supports signal delay|
|InternalSynapse|Computes dynamically weighted signal from source to target neuron. It supports pre-synaptic short-term plasticity.|
|InputUnit|Provides external input for processing in the reservoir. Supports analog and spike-train codings (analog coding supports set of realtime transformations, spike-train coding method is from the "Sparse coding" family). Coded data is then further provided through set of InputNeurons.|
|InputNeuron|Input neuron is the special type of very simple neuron. Its purpose is only to mediate input signal for a synapse|
|HiddenNeuron|Supports both analog and spiking activation functions and can produce analog signal and/or spikes (neuron is able to fire spikes even when stateless analog activation is used). Supports Retainment property of analog activation (leaky integrator). Supports set of different predictors.|
|Reservoir|Provides recurrent network supporting analog and spiking neurons working directly together. Main features: SpectralRadius (for weights of analog, spiking or both neurons), Multiple 3D pools of neurons, Pool to pool connections. It can work as the Echo State Network reservoir, Liquid State Machine reservoir or Mixed reservoir|
|NeuralPreprocessor|Provides data preprocessing to predictors. Supports multiple internal reservoirs. Supports virtual input data associated with predefined signal generators. Supports two input feeding regimes: Continuous and Patterned|
|ReadoutUnit|Readout unit does the Forecast or Classification. Contains cluster of trained readout networks and related important error statistics. Trained unit can contain cluster of the Feed Forward Networks or the Parallel Perceptron Networks|
|ReadoutLayer|Class implements independent readout layer concept useable separatedly or together with reservoir computing preprocessing. Supports x-fold cross validation together with clustering of the trained readout units.|
|StateMachine|The main component. Encapsulates independent NeuralPreprocessor and ReadoutLayer components into the single component and adds support for routing specific predictors and input fields to the specific readout units. Allow to bypass Neural preprocessor and use input directly as a predictors for the readout layer.|

