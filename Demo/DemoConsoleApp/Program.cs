using System;
using System.Collections.Generic;
using RCNet.Demo;
using RCNet.Demo.Log;

using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Reservoir;

namespace RCNet.DemoConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Random rand = new Random(0);
            Interval inputRange = new Interval(-1, 1);
            NeuronPlacement placement = new NeuronPlacement(0, 0, 0, 0);
            InputNeuron inpN = new InputNeuron(0, inputRange);
            for(int i = 0; i < 100; i++)
            {
                double signal = rand.NextBoundedUniformDouble(inputRange.Min, inputRange.Max);
                inpN.Compute(signal, true);
                Console.WriteLine($"inp:{signal} Outp:{inpN.State}");
            }
            Console.ReadLine();


            //IActivationFunction af = ActivationFactory.Create(new ActivationSettings(ActivationFactory.Function.ExpIF));
            ReservoirNeuron neuron = new ReservoirNeuron(placement, ActivationFactory.Create(new ActivationSettings(ActivationFactory.Function.BiLIF)), 0, 0);
            double[] stimuli = new double[500];
            rand.Fill(stimuli, -10, 10, false, RandomClassExtensions.DistributionType.Uniform);
            stimuli.Populate(0);
            stimuli.Populate(5, 0, 100);
            for (int i = 0; i < stimuli.Length; i++)
            {
                neuron.Compute(stimuli[i], true);
                Console.WriteLine($"inp:{stimuli[i]} State:{neuron.State} Signal:{neuron.CurrentSignal}");
            }
            Console.ReadLine();




            //Logging the output to a console
            IOutputLog demoOutputLog = new ConsoleLog();
            //Esn demo
            string esnDemoSettingsFile = @"EsnDemoSettings.xml";
            EsnDemo.RunDemo(demoOutputLog, esnDemoSettingsFile);
            /*
            try
            {
                //Logging the output to a console
                IOutputLog demoOutputLog = new ConsoleLog();
                //Esn demo
                string esnDemoSettingsFile = @"EsnDemoSettings.xml";
                EsnDemo.RunDemo(demoOutputLog, esnDemoSettingsFile);
            }
            catch(Exception e)
            {
                Console.WriteLine();
                while(e != null)
                {
                    Console.WriteLine(e.Message);
                    e = e.InnerException;
                }
            }
            */
            Console.WriteLine("Press Enter.");
            Console.ReadLine();
            return;
        }


    }//Program

}//Namespace
