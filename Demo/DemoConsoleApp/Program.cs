using System;
using System.Collections.Generic;
using RCNet.DemoConsoleApp.Log;

using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM;

namespace RCNet.DemoConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            Random rand = new Random();
            LIF lif = new LIF(15, 0.05, 5, 20, 1);
            for(int i = 0; i < 500; i++)
            {
                double signal = 0;
                if (i < 100)
                {
                    double input = rand.NextDouble(0, 1, false, RandomClassExtensions.DistributionType.Uniform) * rand.NextDouble(0, 1, false, RandomClassExtensions.DistributionType.Uniform);
                    signal = lif.Compute(input);
                }
                else
                {
                    signal = lif.Compute(0);
                }
                Console.WriteLine($"State {lif.InternalState} signal {signal}");
            }
            Console.ReadLine();
            */
            Normalizer nrm = new Normalizer(new Interval(-1, 1), 0, true);
            for (int i = 1; i <= 8; i++)
            {
                nrm.Adjust(i);
            }
            SignalConverter sg = new SignalConverter(new Interval(-1, 1), 3);
            for(int i = 1; i <= 8; i++)
            {
                double normI = nrm.Normalize(i);
                sg.EncodeAnalogValue(normI);
                Console.WriteLine(i + ":");
                for (int j = 0; j < sg.NumOfCodingFractions; j++)
                {
                    Console.WriteLine(sg.FetchSpike());
                }
                Console.WriteLine("---");
            }
            Console.ReadLine();





            //Logging the output to a console
            IOutputLog demoOutputLog = new ConsoleLog();
            //Demo
            string demoSettingsFile = @"DemoSettings.xml";
            SMDemo.RunDemo(demoOutputLog, demoSettingsFile);
            /*
            try
            {
                //Logging the output to a console
                IOutputLog demoOutputLog = new ConsoleLog();
                //Demo
                string demoSettingsFile = @"DemoSettings.xml";
                SMDemo.RunDemo(demoOutputLog, demoSettingsFile);
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
