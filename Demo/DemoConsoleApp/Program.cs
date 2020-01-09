using System;
using System.Collections.Generic;
using RCNet.DemoConsoleApp.Log;

namespace RCNet.DemoConsoleApp
{
    class Program
    {
        static void Main()
        {
            //Research - this is not a part of the demo - it is a free playground
            //(new Research()).Run();

            //Standard execution of the Demo
            try
            {
                //Run the demo
                SMDemo demoEngine = new SMDemo(new ConsoleLog());
                demoEngine.RunDemo(@"DemoSettings.xml");
            }
            catch (Exception e)
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
        }//Main

    }//Program

}//Namespace
