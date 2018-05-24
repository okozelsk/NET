The aim of this project is to make the reservoir computing methods easy to use and available for .net without dependency on external libraries.

Currently implemented components
--------------------------------
Normalizer
    Data normalization/denormalization
    Gaussian standardization

Ordinary Differential Equations (ODE) Numerical Solver
    Euler
    RK4

Activation functions
    Analog
        BentIdentity
        Elliot
        Gaussian
        Identity
        ISRU
        LeakyReLU
        Sigmoid
        Sinc
        Sinusoid
        SoftExponential
        SoftPlus
        TanH
    Spiking
        SimpleIF (Simple Integrate and Fire)
        LeakyIF (Leaky Integrate and Fire)
        ExpIF (Exponential Integrate and Fire)
        AdSimpleIF (Adaptive Simple Integrate and Fire)
        AdExpIF (Adaptive Exponential Integrate and Fire)

Random distributions
    Uniform
    Gaussian

Feed Forward Network
    Xml constructor
    Multiple hidden layers
    Trainers
        Resilient propagation trainer (iRPROP+ variant)
        Linear regression trainer (white noise stabilization)

Parallel Perceptron Network
    Xml constructor
    Trainers
        P-Delta Rule Trainer

Reservoir Neuron
    Input
        Analog
        Spiking
            Converts analog input to spike train
    Hidden
	    Augmented readout state feature
        Analog
            Retainment (leaky integrator) feature
            The second power as augmented readout state
        Spiking
            Firing rate as primary readout state
            Membrane potential as augmented readout state

Reservoir
    Xml constructor
    Provides important internal statistics
    Supports SpectralRadius parameter
    Multiple 3D pools of neurons
	    Pool internal connections
            Supports Euklidean Distance property
	Pool to pool connections
    Supports mixing of analog and spiking neuron pools
        so it can work as
            Echo State Network reservoir
            Liquid State Machine reservoir
            Hybrid (mixed) reservoir

State Machine
    Xml constructor
    Supports multiple internal reservoirs
    Supported task types:
        Prediction
            Time series input
            Output is value prediction
        Classification
            Pattern input
                Supports variable length of patterns
            Output is probability of the class
        Hybrid
            Pattern input
                Supports variable length of patterns
            Output is value prediction

Readout unit
	FF Network or P-Perceptron

Readout layer
    Xml constructor
    Independent on predictors generator (State Machine)
    Supports x-fold cross validation method
        Cluster of readout units per each output field

Bundle normalizer
    Helper for data bundle normalization

Bundle Data loaders (csv)
    PatternDataLoader (Classification or Hybrid task)
    TimeSeriesDataLoader (Prediction task)

Miscellaneous
	Queue (thread safe)
	Hurst exponent estimator (the toughest variant)
	(others)

Demo application
----------------
Main functionality is demonstrated in a simple demo application (/Demo/DemoConsoleApp). Application has no startup parameters, all necessary settins are specified in DemoSettings.xml file. DemoSettings.xml has to be in the same folder as the executable DemoConsoleApp.exe. Application performs sequence of demo cases defined in DemoSettings.xml. Input data has to be stored in a csv file. You can simply modify DemoSettings.xml and configure your own cases to be prformed.


Other information
-----------------
Source code is written in C# 6.0.
Most components are serializable.
Backward compatibility is not guaranteed.
Product documentation will be published on the associated wiki.


Contact:
oldrich.kozelsky@email.cz
