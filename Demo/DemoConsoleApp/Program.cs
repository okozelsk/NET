using Demo.DemoConsoleApp.Examples;
using Demo.DemoConsoleApp.Log;
using Demo.DemoConsoleApp.SM;
using RCNet.Neural.Network.SM.Preprocessing.Input;
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
                Console.WriteLine("  1. State Machine performance demo. Sequentially performs the tasks defined in SMDemoSettings.xml.");
                Console.WriteLine("  2. Feed Forward network trained to solve boolean algebra. Shows use of the FF network stndalone component (no relation to State Machine).");
                Console.WriteLine("  3. TTOO share prices forecast (ESN design from scratch).");
                Console.WriteLine("  4. TTOO share prices forecast (ESN design using StateMachineDesigner).");
                Console.WriteLine("  5. Libras Movement classification (ESN design using StateMachineDesigner).");
                Console.WriteLine("  6. Libras Movement classification (LSM design using StateMachineDesigner, horizontal spiking input encoding).");
                Console.WriteLine("  7. Libras Movement classification (LSM design using StateMachineDesigner, vertical spiking input encoding).");
                Console.WriteLine("  8. Libras Movement classification (LSM design using StateMachineDesigner, analog input direct routing).");
                Console.WriteLine("  9. Libras Movement classification (No preprocessing - alone Readout Layer design using StateMachineDesigner, an indicative benchmark for ESN and LSM).");
                Console.WriteLine("  A. Playground");
                Console.WriteLine("  X. Exit");
                Console.WriteLine();
                Console.WriteLine("  Press the digit or letter of your choice...");
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
                switch (consoleKeyInfo.KeyChar.ToString().ToUpperInvariant())
                {
                    case "1":
                        try
                        {
                            //Run the demo
                            (new SMDemo(new ConsoleLog())).RunDemo(@"./SMDemoSettings.xml");
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "2":
                        try
                        {
                            (new FFNetBoolAlg()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "3":
                        try
                        {
                            (new TTOOForecastFromScratch()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "4":
                        try
                        {
                            (new TTOOForecastDesigner()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "5":
                        try
                        {
                            (new LibrasClassificationESNDesigner()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "6":
                        try
                        {
                            (new LibrasClassificationLSMDesigner()).Run(InputEncoder.InputSpikesCoding.Horizontal);
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "7":
                        try
                        {
                            (new LibrasClassificationLSMDesigner()).Run(InputEncoder.InputSpikesCoding.Vertical);
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "8":
                        try
                        {
                            (new LibrasClassificationLSMDesigner()).Run(InputEncoder.InputSpikesCoding.Forbidden);
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "9":
                        try
                        {
                            (new LibrasClassificationNPBypassedDesigner()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "A":
                        try
                        {
                            (new Playground()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "X":
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
