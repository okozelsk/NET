using System;
using System.Collections.Generic;
using RCNet.DemoConsoleApp.Log;

using RCNet.MathTools;
using RCNet.Neural.Network.SM;

namespace RCNet.DemoConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Interval inputRange = new Interval(-1, 1);
            int spikeTrainLength = 24;
            SpikeTrainConverter converter = new SpikeTrainConverter(inputRange, spikeTrainLength);

            double numToEncode = -0.99587;
            converter.EncodeAnalogValue(numToEncode);
            Console.WriteLine(numToEncode);
            Console.WriteLine(converter.FetchAnalogValue());
            for (int i = 0; i < spikeTrainLength; i++)
            {
                Console.WriteLine(converter.FetchSpike());
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
                EsnDemo.RunDemo(demoOutputLog, demoSettingsFile);
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
