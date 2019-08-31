# Reservoir Computing for .NET (RCNet)
![Reservoir Computing conceptual view](https://github.com/okozelsk/NET/blob/master/RCNet/Docs/Imgs/ReservoirComputing_BasicSchema.png)
<br>
The aim of this project is to make the [reservoir computing](https://en.wikipedia.org/wiki/Reservoir_computing) methods  easy to use and available for .net platform without any other dependencies.
Two main reservoir computing methods are called Echo State Network (ESN) and Liquid State Machine (LSM).
The implemented solution supports both of these methods. However, since ESN and LSM are based on very similar general principles, RCNet brings the option to combine them at the same time. It means the possibility to design complex "hybrid" recurrent networks consisting of spiking and analog neurons working together.
Mutual cooperation of stateless analog and spiking activation functions is enabled by special implementation of hidden neuron, which can fire spikes even in case of stateless analog activation. This approach, I believe, opens up new possibilities and the mutual use of the best of both worlds will be the subject of my further research.
In addition, the Hidden Neuron provides a standardized set of predictors no matter what activation function is used. With RCNet, it is no longer true that the Echo State Network is not capable of quality input signal separation and therefore accurate classification. On the contrary. It now appears that the use of pure analog activations (TanH) and the classic ESN reservoir design is even more powerful and more accurate in classifying tasks than spiking LSM reservoirs.
The main component of RCNet, which is encapsulating whole functionality, is called "**State Machine**".
<br/>
<br/>
Source code is written in C# 6.0. Main documentation is located on project's wiki (https://github.com/okozelsk/NET/wiki) and it is unfortunately obsolete and unfinished.
<br/>
<br/>
I welcome questions, ideas and suggestions for improvements, usage experiences, bug alerts, constructive comments, etc... Please use my email address oldrich.kozelsky@email.cz to contact me.


## Demo application
Main functionality and possibilities are demonstrated in a simple demo application (/Demo/DemoConsoleApp). Application has no startup parameters, all necessary settins are specified in DemoSettings.xml file. DemoSettings.xml has to be in the same folder as the executable DemoConsoleApp.exe. Application performs sequence of tasks defined in DemoSettings.xml. You can easily insert new task or tune existing one by simple modification of DemoSettings.xml. Each task is defined in the xml element "case" and its input data is expected in csv format.


## Components overview

![Reservoir Computing conceptual view](https://github.com/okozelsk/NET/blob/master/RCNet/Docs/Imgs/StateMachine_EntityRelationship.png)
<br/>
(listed in logical order from basic to composite and complex)

### Math
|Component|Description|
|--|--|
|Normalizer|Supports data normalization + standardization and then backward naturalization|
|BasicStat|Provides basic statistics of given data (averages, sum of squares, standard deviation, etc.)|
|ODENumSolver|Implements ordinary differential equations (ODE) numerical solver supporting Euler and RK4 methods|
|Vector|Implements vector of double values supporting basic mathematical operations|
|Matrix|Implements matrix of double values supporting basic mathematical operations. Contains buit-in Power Iteration method for the largest eigen value quick estimation|
|EVD|Full eigen values and vectors decomposition of a squared matrix|
|SVD|Singular values decomposition of a matrix|
|QRD|QR decomposition of a matrix|
|LUD|LU decomposition of a squared matrix|
|ParamSeeker|Implements an error driven iterative search for the best value of a given parameter|
|HurstExpEstim|Implements Hurst exponent estimator. It can be used to evaluate level of data randomness|
|"RandomValue"|Supports Uniform and Gaussian distributions|
|Others|Set of small additional helper components like PhysUnit, Interval, Bitwise, Combinatorics, Factorial, WeightedAvg, ...|

### Signal generators
|Component|Description|
|--|--|
|PulseGenerator|Generates constant pulses at a specified frequency|
|MackeyGlassGenerator|Generates Mackey-Glass chaotic signal|
|RandomGenerator|Generates random signal|
|SinusoidalGenerator|Generates sinusoidal signal|

### XML handling
|Component|Description|
|--|--|
|ElemValidator|Implements validation of alone xml element against specified type defined in xsd. It complements the apparently missing .net method and is necessary to comply with the overall RCNet xml settings concept.|
|DocValidator|Provides the xml loading/validation functionalities|

### Data handling
|Component|Description|
|--|--|
|DelimitedStringValues|Helper encoder and decoder of data in csv format|
|PatternBundle|Bundle of patterns of input data vectors and corresponding desired output vectors (n:1). Supports upload from csv file|
|VectorBundle|Bundle of input data vectors and corresponding desired output vectors (1:1). Supports upload from csv file|
|ResultComparativeBundle|Bundle of computed output vectors and desired output vectors (1:1)|
|BundleNormalizer|Helper class for easy standardization + normalization and backward naturalization of data within PatternBundle or VectorBundle|

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
|LeakyIF|Leaky Integrate and Fire activation function|
|ExpIF|Exponential Integrate and Fire activation function|
|AdExpIF|Adaptive Exponential Integrate and Fire activation function|
|IzhikevichIF|Izhikevich Integrate and Fire activation function (model "one fits all")|
See the [wiki pages.](https://en.wikipedia.org/wiki/Biological_neuron_model)

### Non-recurrent networks and trainers
|Component|Description|
|--|--|
|FeedForwardNetwork|Implements the feed forward network supporting multiple hidden layers|
|RPropTrainer|Resilient propagation (iRPROP+) trainer of the feed forward network|
|QRDRegrTrainer|Implements the linear regression (QR decomposition) trainer of the feed forward network. This is the special case trainer for FF network having no hidden layers and Identity output activation function|
|RidgeRegrTrainer|Implements the ridge linear regression trainer of the feed forward network. This is the special case trainer for FF network having no hidden layers and Identity output activation function|
|ElasticRegrTrainer|Implements the elastic net trainer of the feed forward network. This is the special case trainer for FF network having no hidden layers and Identity output activation function|
|ParallelPerceptron|Implements the parallel perceptron network|
|PDeltaRuleTrainer|P-Delta rule trainer of the parallel perceptron network|

### State Machine components
|Component|Description|
|--|--|
|InputSynapse|Computes constantly weighted signal from external input and passes it to target neuron. Supports signal delay|
|InternalSynapse|Computes dynamically weighted signal from source to target neuron. It supports pre-synaptic short-term plasticity.|
|InputNeuron|Input neuron is the special type of very simple neuron. Its purpose is only to mediate input analog value for a synapse|
|HiddenNeuron|Supports both analog and spiking activation functions and can produce analog signal and/or spikes (neuron is able to fire spikes even when stateless analog activation is used). Supports Retainment property of analog activation (leaky integrator). Supports set of different predictors.|
|Reservoir|Provides recurrent network supporting analog and spiking neurons working directly together. Main features: SpectralRadius (for weights of analog, spiking or both neurons), Multiple 3D pools of neurons, Pool to pool connections. It can work as the Echo State Network reservoir, Liquid State Machine reservoir or Mixed reservoir|
|NeuralPreprocessor|Provides data preprocessing to predictors. Supports multiple internal reservoirs. Supports virtual input data associated with predefined signal generators. Supports two input feeding regimes: Continuous and Patterned|
|ReadoutUnit|Readout unit does the Forecast or Classification. Contains trained output unit and related important error statistics. Trained unit can be the Feed Forward Network or the Parallel Perceptron Network|
|ReadoutLayer|Class implements common readout layer concept for the reservoir computing methods. Supports x-fold cross validation method and clustering of the trained readout units.|
|StateMachine|The main component. Encaptulates independent NeuralPreprocessor and ReadoutLayer fuctionalities into the single logical unit.|

