The aim of the project is to make the reservoir computing methods easy to use and available for .net without dependency on external libraries.

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
	Sigmoid (Logistic, Softstep)
	Gaussian
Feed Forward Network
	Multiple hidden layers
Network Trainers
	Linear regression trainer
	Resilient propagation trainer (iRPROP+ variant)
Analog Reservoir
	Random, Ring and Doubly Twisted Thoroidal internal connection topologies
	Context neuron feature
	Retainment neurons (leaky integrators) feature
	Augmented states feature
Echo State Network
	Feedback
	Multiple internal reservoirs
	Readout layer consists of the feed forward network for every output field
	Esn does not support SpectralRadius parameter (low added value)
	Esn provides the internal statistics
	User should simply tune parameters and monitor the statistics and results

Demo application
----------------
All the functionality is demonstrated in a simple demo application (/Demo/DemoConsoleApp). Application has no startup parameters, all necessary settins are specified in EsnDemoSettings.xml file. EsnDemoSettings.xml has to be in the same folder as the executable DemoConsoleApp.exe. Application performs training-->prediction operations sequence for each demo case defined in EsnDemoSettings.xml. Input time series data has to be stored in a file (csv format). You can simply modify EsnDemoSettings.xml and configure your own training-->prediction sessions.


Other information
-----------------
Very soon an official release will be published and from this point will be also kept the backward compatibility. Product documentation will be published on the associated wiki.
Project's next ambition is to implement the Liquid State Machine (LSM).


Contact:
oldrich.kozelsky@email.cz
