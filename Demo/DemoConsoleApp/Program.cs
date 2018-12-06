using System;
using System.Collections.Generic;
using RCNet.DemoConsoleApp.Log;

namespace RCNet.DemoConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Research
            Research r = new Research();
            r.Run();

            //Demo standard execution
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
            Console.WriteLine("Press Enter.");
            Console.ReadLine();
            return;
        }


    }//Program

}//Namespace
