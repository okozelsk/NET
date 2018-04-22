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
