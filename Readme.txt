The aim of the project is to make the reservoir computing methods available for .net without binding other libraries (Python, MatLab, etc.). The first (already implemented) method is the Echo State Network (aka ESN). Project's next ambition is to implement the Liquid State Machine (aka LSM).

Currently implemented components:
	Normalizer
		data normalizer/denormalizer with built-in gausse data standardization
	Activation functions
		Tanh, Elliot, Sinusoid, Identity
	AnalogReservoir
		supporting internal connection topologies: Random, Ring, Doubly Twisted Thoroidal
		supporting Context neuron and Retirement neurons features
		supporting Augmented states
	FF/BasicNetwork
		Feed Forward Network
	LinRegrTrainer
		Linear Regression trainer
	RPROPTrainer
		Resilient Propagation trainer iRPROP+
	ESN
		Echo State Network
		supporting multiple internal reservoirs
		supporting Feedback

The functionality of all components is demonstrated in a simple demo application (/Demo/ESNDemoConsoleApp)
	Application has no startup parameters. All the settins are in ESNDemoSettings.xml file. File has to be in the same folder as the executable.
	The input data for each prediction session (demo case) is the csv file.
	You can simply modify ESNDemoSettings.xml to configure your own prediction session (Input data, ESN and Regression settings).
		




Contact:
oldrich.kozelsky@email.cz
