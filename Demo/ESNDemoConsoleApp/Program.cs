using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKOSW.Demo;
using OKOSW.Demo.Log;

namespace OKOSW.EsnDemoConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string demoSettingsFile = @"EsnDemoSettings.xml";
            IOutputLog demoOutputLog = new ConsoleLog();
            EsnDemo.RunDemo(demoOutputLog, demoSettingsFile);
            Console.ReadLine();
            return;
        }
    }
}
