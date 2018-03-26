The aim of the project is to make the reservoir computing methods easy to use and available for .net without
dependency on external libraries.

Currently implemented components:
	Normalizer
		data normalizer/denormalizer with built-in gausse data standardization
	Activation functions
		TanH, Elliot (Softsign), Sinusoid, Identity, ISRU (Inverse Squere Root Unit),
		Sigmoid (Logistic, Softstep), Gaussian
	Feed Forward Neural Network
		supporting multiple hidden layers
	Trainers
		Linear regression trainer
		Resilient propagation trainer (iRPROP+ variant)
	Analog Reservoir
		supporting internal connection topologies: Random, Ring, Doubly Twisted Thoroidal
		supporting Context neuron, Retainment neurons (leaky integrators) and Augmented states features
	Echo State Network
		supporting multiple internal reservoirs
		supporting feedback
		readout layer consists of FF network for every output field (with or without hidden layers)
		ESN does not support SpectralRadius parameter and it will not be added.
			instead of SpectralRadius parameter ESN provides detailed statistics
			of reservoirs' neurons states. User should simply tune weight scales
			and watch statistics/results to select right scales and also to avoid
			reservoirs' neurons oversaturation.
			
Demo application
	All the functionality is demonstrated in a simple demo application (/Demo/DemoConsoleApp).
	Application has no startup parameters, all necessary settins are specified
	in EsnDemoSettings.xml file.
	EsnDemoSettings.xml has to be in the same folder as the executable DemoConsoleApp.exe.
	Application performs training-->prediction operations sequence for each demo case
	defined in EsnDemoSettings.xml.
	Input time series data has to be stored in a file (csv format).
	You can simply modify EsnDemoSettings.xml and configure your own training-->prediction sessions.


Very soon an official release will be published and from this point
will be also kept the backward compatibility. Product documentation
will be published on the associated wiki.


Project's next ambition is to implement the Liquid State Machine (LSM).



Contact:
oldrich.kozelsky@email.cz
