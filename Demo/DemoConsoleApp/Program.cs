using Demo.DemoConsoleApp.Examples;
using Demo.DemoConsoleApp.Log;
using Demo.DemoConsoleApp.SM;
using System;

namespace Demo.DemoConsoleApp
{
    class Program
    {
        static void Main()
        {
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
                Console.WriteLine("  9. Playground");
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
                            (new SMDemo(new ConsoleLog())).RunDemo(@"SM\SMDemoSettings.xml");
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case '2':
                        try
                        {
                            (new FFNetBoolAlg()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case '3':
                        try
                        {
                            (new TTOOForecastFromScratch()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case '4':
                        try
                        {
                            (new TTOOForecastDesigner()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case '5':
                        try
                        {
                            (new LibrasClassificationESNDesigner()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case '6':
                        try
                        {
                            (new LibrasClassificationLSMDesigner()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case '9':
                        try
                        {
                            (new Playground()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
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

        /// <summary>
        /// Displays exception content
        /// </summary>
        /// <param name="e">Exception to be displayed</param>
        private static void ReportException(Exception e)
        {
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------------------");
            while (e != null)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                e = e.InnerException;
                Console.WriteLine("--------------------------------------------------------------");
            }
            return;
        }

    }//Program

}//Namespace
