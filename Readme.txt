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
	RPopPTrainer
		Resilient Propagation trainer iRPROP+
	Esn
		Echo State Network
		supporting multiple internal reservoirs
		supporting Feedback

The functionality is demonstrated in a simple demo application (/Demo/EsnDemoConsoleApp).
Application has no startup parameters, all necessary settins are specified in EsnDemoSettings.xml file.
EsnDemoSettings.xml has to be in the same folder as the executable EsnDemoConsoleApp.exe.
Application performs training-->prediction operations sequence for each demo case defined in EsnDemoSettings.xml.
Necessary input time series data has to be stored in csv format.
You can simply modify EsnDemoSettings.xml to configure your own training-->prediction sessions.



Contact:
oldrich.kozelsky@email.cz
