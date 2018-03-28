using System;
using RCNet.Demo;
using RCNet.Demo.Log;

namespace RCNet.DemoConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Output logging to console
            IOutputLog demoOutputLog = new ConsoleLog();
            //Esn demo
            string esnDemoSettingsFile = @"EsnDemoSettings.xml";
            EsnDemo.RunDemo(demoOutputLog, esnDemoSettingsFile);
            Console.ReadLine();
            return;
        }

    }//Program

}//Namespace
