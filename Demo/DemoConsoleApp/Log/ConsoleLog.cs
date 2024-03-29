﻿using System;
using System.Text;

namespace Demo.DemoConsoleApp.Log
{
    /// <summary>
    /// Implements the simple output log using system console.
    /// </summary>
    public class ConsoleLog : IOutputLog
    {
        //Constants
        public const int ConsoleBufferMinWidth = 500;
        public const int ConsoleBufferMinHeight = 1000;

        //Attributes
        private int _lastMessageLength;
        private int _lastCursorLeft;
        private int _lastCursorTop;

        /// <summary>
        /// Constructs simple output journal using console
        /// </summary>
        public ConsoleLog()
        {
#if Windows
            //Set console buffer size
            Console.SetBufferSize(Math.Max(ConsoleBufferMinWidth, Console.BufferWidth), Math.Max(ConsoleBufferMinHeight, Console.BufferHeight));
            //Adjust console window position and size
            Console.WindowLeft = 0;
            Console.WindowTop = 0;
            Console.WindowWidth = Console.LargestWindowWidth;
#endif
            //Clear the console
            Console.Clear();
            //Store current cursor position
            StoreCursor();
            //Reset last message length
            _lastMessageLength = 0;
            return;
        }

        //Methods
        /// <summary>
        /// Restores the cursor position.
        /// </summary>
        private void RestoreCursor()
        {
            Console.CursorLeft = _lastCursorLeft;
            Console.CursorTop = _lastCursorTop;
            return;
        }

        /// <summary>
        /// Stores the current cursor position.
        /// </summary>
        private void StoreCursor()
        {
            _lastCursorLeft = Console.CursorLeft;
            _lastCursorTop = Console.CursorTop;
            return;
        }

        /// <summary>
        /// Writes a message to the system console.
        /// </summary>
        /// <param name="message">The message to be written to system console.</param>
        /// <param name="replaceLastMessage">Specifies whether to replace text of the previous message.</param>
        public void Write(string message, bool replaceLastMessage = false)
        {
            if (replaceLastMessage)
            {
                RestoreCursor();
                if (_lastMessageLength > 0)
                {
                    StringBuilder emptyMsg = new StringBuilder(_lastMessageLength);
                    for (int i = 0; i < _lastMessageLength; i++)
                    {
                        emptyMsg.Append(" ");
                    }
                    Console.Write(emptyMsg.ToString());
                    RestoreCursor();
                }
                Console.Write(message);
            }
            else
            {
                Console.WriteLine();
                StoreCursor();
                Console.Write(message);
            }
            _lastMessageLength = message.Length;
            return;
        }
    }//ConsoleLog

}//Namespace
