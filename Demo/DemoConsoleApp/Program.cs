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
            while (true)
            {
                //Main menu
                Console.Clear();
                Console.WriteLine("Main menu:");
                Console.WriteLine("  1. State Machine performance demo (execution of the tasks defined in the SMDemoSettings.xml)");
                Console.WriteLine("  2. Feed Forward network trained to solve boolean algebra");
                Console.WriteLine("  3. TTOO share prices forecast (State Machine ESN setup from scratch)");
                Console.WriteLine("  4. TTOO share prices forecast (State Machine ESN setup using StateMachineDesigner)");
                Console.WriteLine("  5. Libras Movement classification (State Machine ESN setup using StateMachineDesigner)");
                Console.WriteLine("  6. Libras Movement classification (State Machine LSM setup using StateMachineDesigner)");
                Console.WriteLine("  0. Exit");
                Console.WriteLine();
                Console.WriteLine("  Press the digit...");
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
                            (new FFNetBoolAlg()).Run();
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

                    case '3':
                        try
                        {
                            (new TTOOForecastFromScratch()).Run();
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

                    case '4':
                        try
                        {
                            (new TTOOForecastDesigner()).Run();
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

                    case '5':
                        try
                        {
                            (new LibrasClassificationESNDesigner()).Run();
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

                    case '6':
                        try
                        {
                            (new LibrasClassificationLSMDesigner()).Run();
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

                    case '0':
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
