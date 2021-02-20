using Demo.DemoConsoleApp.Examples.NonRecurrent;
using Demo.DemoConsoleApp.Examples.SM;
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
            //Run the root menu
            RootMenu();
        }//Main

        private static void RootMenu()
        {
            bool exit = false;
            while (!exit)
            {
                bool wait = true;
                //Root menu
                Console.Clear();
                Console.WriteLine("Root menu:");
                Console.WriteLine("  1. State Machine performance demo (it sequentially performs the tasks defined in SMDemoSettings.xml).");
                Console.WriteLine("  2. State Machine code examples sub-menu...");
                Console.WriteLine("  3. Non-Recurrent networks code examples sub-menu...");
                Console.WriteLine("  P. Playground");
                Console.WriteLine("  X. Exit the demo application");
                Console.WriteLine();
                Console.WriteLine("  Press the digit or letter of your choice...");
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
                Console.Clear();
                switch (consoleKeyInfo.KeyChar.ToString().ToUpperInvariant())
                {
                    case "1":
                        try
                        {
                            //Run State Machine performance demo.
                            (new SMDemo(new ConsoleLog())).RunDemo(@"./SMDemoSettings.xml");
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "2":
                        //State Machine code examples sub menu
                        SMCodeExamplesMenu();
                        wait = false;
                        break;

                    case "3":
                        NRNetCodeExamplesMenu();
                        wait = false;
                        break;

                    case "P":
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
                        exit = true;
                        wait = false;
                        break;

                    default:
                        Console.WriteLine();
                        Console.WriteLine("Undefined choice.");
                        break;

                }//Switch choice

                //Wait for Enter to loop the menu
                if (wait)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press Enter to return to menu...");
                    Console.ReadLine();
                }

            }//Menu loop
            return;
        }

        private static void SMCodeExamplesMenu()
        {
            bool exit = false;
            while (!exit)
            {
                bool wait = true;
                //Menu
                Console.Clear();
                Console.WriteLine("State Machine code examples menu:");
                Console.WriteLine("  1. TTOO share prices forecast (ESN design from scratch).");
                Console.WriteLine("  2. TTOO share prices forecast (ESN design using StateMachineDesigner).");
                Console.WriteLine("  3. Libras Movement classification (ESN design using StateMachineDesigner).");
                Console.WriteLine("  4. Libras Movement classification (LSM design using StateMachineDesigner, horizontal spiking input encoding).");
                Console.WriteLine("  5. Libras Movement classification (LSM design using StateMachineDesigner, vertical spiking input encoding).");
                Console.WriteLine("  6. Libras Movement classification (LSM design using StateMachineDesigner, direct routing of analog input).");
                Console.WriteLine("  7. Libras Movement classification (bypassed preprocessing design using StateMachineDesigner).");
                Console.WriteLine("  X. Back to Root menu");
                Console.WriteLine();
                Console.WriteLine("  Press the digit or letter of your choice...");
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
                Console.Clear();
                switch (consoleKeyInfo.KeyChar.ToString().ToUpperInvariant())
                {
                    case "1":
                        try
                        {
                            (new Forecast_TTOO_ESN_FromScratch()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "2":
                        try
                        {
                            (new Forecast_TTOO_ESN_SMD()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "3":
                        try
                        {
                            (new Classification_LibrasMovement_ESN_SMD()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "4":
                        try
                        {
                            (new Classification_LibrasMovement_LSM_SMD()).Run(InputEncoder.InputSpikesCoding.Horizontal);
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "5":
                        try
                        {
                            (new Classification_LibrasMovement_LSM_SMD()).Run(InputEncoder.InputSpikesCoding.Vertical);
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "6":
                        try
                        {
                            (new Classification_LibrasMovement_LSM_SMD()).Run(InputEncoder.InputSpikesCoding.Forbidden);
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "7":
                        try
                        {
                            (new Classification_LibrasMovement_NPBypass_SMD()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "X":
                        exit = true;
                        wait = false;
                        break;

                    default:
                        Console.WriteLine();
                        Console.WriteLine("Undefined choice.");
                        break;

                }//Switch choice

                //Wait for Enter to loop the menu
                if (wait)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press Enter to return to menu...");
                    Console.ReadLine();
                }

            }//Menu loop
            return;
        }

        private static void NRNetCodeExamplesMenu()
        {
            bool exit = false;
            while (!exit)
            {
                bool wait = true;
                //Menu
                Console.Clear();
                Console.WriteLine("Non-Recurrent networks code examples menu:");
                Console.WriteLine("  1. Feed Forward network trained to solve boolean algebra.");
                Console.WriteLine("  2. Classifications performed by Network Cluster component.");
                Console.WriteLine("  X. Back to Root menu");
                Console.WriteLine();
                Console.WriteLine("  Press the digit or letter of your choice...");
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
                Console.Clear();
                switch (consoleKeyInfo.KeyChar.ToString().ToUpperInvariant())
                {
                    case "1":
                        try
                        {
                            (new FeedForwardNetwork_BooleanAlgebra()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "2":
                        try
                        {
                            (new Classification_TNRNetCluster_FromScratch()).Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "X":
                        exit = true;
                        wait = false;
                        break;

                    default:
                        Console.WriteLine();
                        Console.WriteLine("Undefined choice.");
                        break;

                }//Switch choice

                //Wait for Enter to loop the menu
                if (wait)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press Enter to return to menu...");
                    Console.ReadLine();
                }

            }//Menu loop
            return;
        }

        /// <summary>
        /// Displays the exception content.
        /// </summary>
        /// <param name="e">An exception to be displayed.</param>
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
