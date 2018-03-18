using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKOSW.Demo;
using OKOSW.Demo.Log;

namespace ESNDemoConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string demoSettingsFile = @"ESNDemoSettings.xml";
            IOutputLog logger = new ConsoleLog();
            ESNDemo.RunDemo(logger, demoSettingsFile);
            Console.ReadLine();
            return;
        }
    }
}
