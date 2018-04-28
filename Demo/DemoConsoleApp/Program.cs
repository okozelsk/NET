using System;
using System.Collections.Generic;
using RCNet.DemoConsoleApp.Log;

using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM;

namespace RCNet.DemoConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            LIF lif = new LIF(0.95, 15, -70, -68, -62, 0);
            for(int i = 0; i < 500; i++)
            {
                double signal = 0;
                if (i < 100)
                {
                    signal = lif.Compute(0.1);
                }
                else
                {
                    signal = lif.Compute(0);
                }
                Console.WriteLine($"State {lif.InternalState} signal {signal}");
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
