using System;
using System.Collections.Generic;
using RCNet.Demo;
using RCNet.Demo.Log;

namespace RCNet.DemoConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
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
