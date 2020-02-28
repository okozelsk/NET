using System;
using System.Collections.Generic;
using Demo.DemoConsoleApp.Examples;
using Demo.DemoConsoleApp.Log;
using Demo.DemoConsoleApp.SM;

namespace Demo.DemoConsoleApp
{
    class Program
    {
        static void Main()
        {
            //Research - this is not a part of the demo - it is a free playground
            //(new Research()).Run();
            //
            //(new ForecastFromScratch()).Run();
            while (true)
            {
                //Main menu
                Console.Clear();
                Console.WriteLine("Main menu:");
                Console.WriteLine("  1. State Machine demo tasks (execution of SMDemoSettings.xml)");
                Console.WriteLine("  2. State Machine forecast setup from scratch (code example)");
                Console.WriteLine("  9. Exit");
                Console.WriteLine();
                Console.WriteLine("  Press digit...");
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
                switch (consoleKeyInfo.KeyChar)
                {
                    case '1':
                        try
                        {
                            //Run the demo
                            SMDemo demoEngine = new SMDemo(new ConsoleLog());
                            demoEngine.RunDemo(@"SM\SMDemoSettings.xml");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine();
                            while (e != null)
                            {
                                Console.WriteLine(e.Message);
                                e = e.InnerException;
                            }
                        }
                        break;

                    case '2':
                        try
                        {
                            (new ForecastFromScratch()).Run();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine();
                            while (e != null)
                            {
                                Console.WriteLine(e.Message);
                                e = e.InnerException;
                            }
                        }
                        break;

                    case '9':
                        return;

                    default:
                        break;

                }//Switch choice
                //Loop the menu
                Console.WriteLine();
                Console.WriteLine("Press Enter to return to menu...");
                Console.ReadLine();
            }//Menu loop

        }//Main

    }//Program

}//Namespace
