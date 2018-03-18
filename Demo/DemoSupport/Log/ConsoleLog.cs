using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKOSW.Demo.Log
{
    public class ConsoleLog : IOutputLog
    {
        //Constants
        public const int CONSOLE_MIN_WIDTH = 160;
        public const int CONSOLE_MIN_HEIGHT = 50;
        public const int CONSOLE_BUFFER_MIN_WIDTH = CONSOLE_MIN_WIDTH*2;
        public const int CONSOLE_BUFFER_MIN_HEIGHT = 1000;

        //Attributes
        private int m_lastMessageLength;
        private int m_lastCursorLeft;
        private int m_lastCursorTop;

        /// <summary>
        /// Constructs console log and sets required console sizes (if necessary)
        /// </summary>
        public ConsoleLog()
        {
            //Ensure console min sizes
            Console.BufferWidth = Math.Max(CONSOLE_BUFFER_MIN_WIDTH, Console.BufferWidth);
            Console.BufferHeight = Math.Max(CONSOLE_BUFFER_MIN_HEIGHT, Console.BufferHeight);
            Console.WindowWidth = Math.Max(CONSOLE_MIN_WIDTH, Console.WindowWidth);
            Console.WindowHeight = Math.Max(CONSOLE_MIN_HEIGHT, Console.WindowHeight);
            Console.Clear();
            //Store current cursor position
            StoreCursor();
            //Reset last message length
            m_lastMessageLength = 0;
            return;
        }

        //Methods
        /// <summary>
        /// Restores cursor position
        /// </summary>
        private void RestoreCursor()
        {
            Console.CursorLeft = m_lastCursorLeft;
            Console.CursorTop = m_lastCursorTop;
            return;
        }

        /// <summary>
        /// Stores current cursor position
        /// </summary>
        private void StoreCursor()
        {
            m_lastCursorLeft = Console.CursorLeft;
            m_lastCursorTop = Console.CursorTop;
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
                if (m_lastMessageLength > 0)
                {
                    StringBuilder emptyMsg = new StringBuilder(m_lastMessageLength);
                    for (int i = 0; i < m_lastMessageLength; i++)
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
            m_lastMessageLength = message.Length;
            return;
        }

    }//ConsoleLog
}//Namespace
