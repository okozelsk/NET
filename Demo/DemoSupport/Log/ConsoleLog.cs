using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKOSW.Demo.Log
{
    /// <summary>
    /// Simple interface to allow to display output information
    /// </summary>
    public class ConsoleLog : IOutputLog
    {
        //Constants
        public const int ConsoleMinWidth = 160;
        public const int ConsoleMinHeight = 50;
        public const int ConsoleBufferMinWidth = ConsoleMinWidth*2;
        public const int ConsoleBufferMinHeight = 1000;

        //Attributes
        private int _lastMessageLength;
        private int _lastCursorLeft;
        private int _lastCursorTop;

        /// <summary>
        /// Constructs console log and sets required console sizes (if necessary)
        /// </summary>
        public ConsoleLog()
        {
            //Ensure console min sizes
            Console.BufferWidth = Math.Max(ConsoleBufferMinWidth, Console.BufferWidth);
            Console.BufferHeight = Math.Max(ConsoleBufferMinHeight, Console.BufferHeight);
            Console.WindowWidth = Math.Max(ConsoleMinWidth, Console.WindowWidth);
            Console.WindowHeight = Math.Max(ConsoleMinHeight, Console.WindowHeight);
            Console.Clear();
            //Store current cursor position
            StoreCursor();
            //Reset last message length
            _lastMessageLength = 0;
            return;
        }

        //Methods
        /// <summary>
        /// Restores cursor position
        /// </summary>
        private void RestoreCursor()
        {
            Console.CursorLeft = _lastCursorLeft;
            Console.CursorTop = _lastCursorTop;
            return;
        }

        /// <summary>
        /// Stores current cursor position
        /// </summary>
        private void StoreCursor()
        {
            _lastCursorLeft = Console.CursorLeft;
            _lastCursorTop = Console.CursorTop;
            return;
        }

        /// <summary>
        /// Writes message to console
        /// </summary>
        /// <param name="message">Message to be written to output</param>
        /// <param name="replaceLastMessage">Indicates if to replace last written message by this new message</param>
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
