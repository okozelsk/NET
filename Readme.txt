The aim of this project is to make the reservoir computing methods easy to use and available for .net without dependency on external libraries.

Currently implemented components
--------------------------------
Normalizer
	Data normalization/denormalization
	Gausse standardization
Activation functions
	TanH
	Elliot (Softsign)
	Sinusoid
	Identity (Linear)
	ISRU (Inverse Squere Root Unit)
	Gaussian
Feed Forward Network
	Multiple hidden layers
	Trainers
		Resilient propagation trainer (iRPROP+ variant)
		Linear regression trainer
Parallel Perceptron Network
	Parallel Perceptron Trainer (p-delta rule)
Analog Reservoir
	Supported topologies:
		Random
		Ring
		Doubly Twisted Thoroidal
	Context neuron feature
	Retainment neurons (leaky integrators) feature
	Augmented states feature
Echo State Network
	Multiple internal reservoirs
	Esn does not support SpectralRadius parameter (low added value)
		but provides useful internal statistics
	Supported task types:
		Prediction
		Classification
			Supports variable length of patterns
Readout layer
	Independent on predictors generator
	Supports x-fold cross validation method
	Clusters of readout units
		Cluster of readout units per each output field
			Feed Forward Network or Parallel Perceptron
Bundle normalizer
	Helper for normalization of data in a bundle

Data loaders (csv)
	csv data format for specific task type
		Prediction (time series)
		Classification

Demo application
----------------
Functionality is demonstrated in a simple demo application (/Demo/DemoConsoleApp). Application has no startup parameters, all necessary settins are specified in EsnDemoSettings.xml file. EsnDemoSettings.xml has to be in the same folder as the executable DemoConsoleApp.exe. Application performs sequence of demo cases defined in EsnDemoSettings.xml. Input data has to be stored in a file in csv format. You can simply modify EsnDemoSettings.xml and configure your own cases to be prformed.


Other information
-----------------
Source code is written in C# 6.0.
Backward compatibility is not guaranteed.
Product documentation will be published on the associated wiki.
Project's next step is to implement the Liquid State Machine (LSM).

Contact:
oldrich.kozelsky@email.cz
