The aim of this project is to make the reservoir computing methods easy to use and available for .net without dependency on external libraries.

Currently implemented components
--------------------------------
Normalizer
	Data normalization/denormalization
	Gaussian standardization
Activation functions
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
Analog Reservoir
	Xml constructor
	Provides important internal statistics
	Supports SpectralRadius parameter (but high comp. cost)
		It can be suppressed by specifying the NA code
	Supported topologies:
		Random
		Ring
		Doubly Twisted Thoroidal
	Context neuron feature
	Retainment neurons (leaky integrators) feature
	Augmented states feature
Echo State Network
	Xml constructor
	Multiple internal reservoirs
	Provides important internal reservoirs statistics
	Supported task types:
		Prediction
			Time series input
			Readout unit output is value prediction
		Classification
			Pattern input
				Supports variable length of patterns
			Readout unit output is probability of class
		Hybrid
			Pattern input
				Supports variable length of patterns
			Readout unit output is value prediction
Readout layer
	Xml constructor
	Independent on predictors generator (Esn, ...)
	Supports x-fold cross validation method
	Clusters of readout units
		Cluster of readout units per each output field
			Cluster of FF Networks or P-Perceptrons
Bundle normalizer
	Helper for data bundle normalization
Bundle Data loaders (csv)
	PatternDataLoader (Classification or Hybrid task)
	TimeSeriesDataLoader (Prediction task)
Miscellaneous
	Queue (thread safe)
	Hurst exponent estimator (the toughest variant)
	And others :-)

Demo application
----------------
Main functionality is demonstrated in a simple demo application (/Demo/DemoConsoleApp). Application has no startup parameters, all necessary settins are specified in EsnDemoSettings.xml file. EsnDemoSettings.xml has to be in the same folder as the executable DemoConsoleApp.exe. Application performs sequence of demo cases defined in EsnDemoSettings.xml. Input data has to be stored in a file in csv format. You can simply modify EsnDemoSettings.xml and configure your own cases to be prformed.


Other information
-----------------
Source code is written in C# 6.0.
Backward compatibility is not guaranteed.
Product documentation will be published on the associated wiki.
Project's next step is to implement the Liquid State Machine (LSM).

Contact:
oldrich.kozelsky@email.cz
