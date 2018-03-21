Aim of this project is to enable reservoir computing for .NET without any other dependencies.

Currently implemented:
	Normalizer (data normalizer/denormalizer supporting gausse data standardization)
	Activation functions (Tanh, Elliot, Sinusoid, Identity)
	AnalogReservoir
		(supporting internal connection topologies: Random, Ring, Doubly Twisted Thoroidal)
		(supporting Context neuron and Retirement neurons features)
		(supporting Augmented states)
	FF/BasicNetwork (Feed Forward Network)
	LinRegrTrainer (Linear Regression trainer)
	RPROPTrainer (Resilient Propagation trainer iRPROP+)
	ESN (Echo State Network supporting multiple internal reservoirs)
		(supporting Feedback)

Usage is demonstrated in simple demo console application (/Demo/ESNDemoConsoleApp)




Contact:
oldrich.kozelsky@email.cz
