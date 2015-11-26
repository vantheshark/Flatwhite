using System;

namespace Flatwhite
{
    /// <summary>
    /// Provide methods to write error
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log an exception
        /// </summary>
        /// <param name="ex"></param>
        void Error(Exception ex);
        /// <summary>
        /// Log a message and exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        void Error(string message, Exception ex);

        /// <summary>
        /// Log info
        /// </summary>
        /// <param name="message"></param>
        void Info(string message);
    }

    /// <summary>
    /// Console logger
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        public void Error(Exception ex)
        {
            Console.WriteLine($"{DateTime.Now}: {ex.Message}");
        }

        public void Error(string message, Exception ex)
        {
            Console.WriteLine($"{DateTime.Now}: {message}\r\n\t\t{ex.Message}");
        }

        public void Info(string message)
        {
            Console.WriteLine($"{DateTime.Now}: {message}");
        }
    }

    internal class NullLogger : ILogger
    {
        public void Error(Exception ex)
        {
        }

        public void Error(string message, Exception ex)
        {
        }

        public void Info(string message)
        {
        }
    }
}