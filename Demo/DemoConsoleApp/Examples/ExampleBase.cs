using Demo.DemoConsoleApp.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.DemoConsoleApp.Examples
{
    /// <summary>
    /// Common base class for all code examples.
    /// </summary>
    public class ExampleBase
    {
        //Attributes
        protected readonly IOutputLog _log;

        //Constructor
        protected ExampleBase()
        {
            _log = new ConsoleLog();
            return;
        }

    }//ExampleBase

}//Namespace
