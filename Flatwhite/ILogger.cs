using System;
using System.Diagnostics.CodeAnalysis;

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
        /// Log a warning message
        /// </summary>
        /// <param name="message"></param>
        void Warn(string message);

        /// <summary>
        /// Log info
        /// </summary>
        /// <param name="message"></param>
        void Info(string message);
    }

    /// <summary>
    /// Console logger
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ConsoleLogger : ILogger
    {
        /// <summary>
        /// Log an exception
        /// </summary>
        /// <param name="ex"></param>
        public void Error(Exception ex)
        {
            Console.WriteLine($"{DateTime.Now}: {ex.Message}");
        }

        /// <summary>
        /// Log a message and exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public void Error(string message, Exception ex)
        {
            Console.WriteLine($"{DateTime.Now}: {message}\r\n\t\t{ex.Message}");
        }

        /// <summary>
        /// Log warning
        /// </summary>
        /// <param name="message"></param>
        public void Warn(string message)
        {
            Console.WriteLine($"{DateTime.Now}: {message}");
        }

        /// <summary>
        /// Log info
        /// </summary>
        /// <param name="message"></param>
        public void Info(string message)
        {
            Console.WriteLine($"{DateTime.Now}: {message}");
        }
    }

    [ExcludeFromCodeCoverage]
    internal class NullLogger : ILogger
    {
        public void Error(Exception ex)
        {
        }

        public void Error(string message, Exception ex)
        {
        }

        public void Warn(string message)
        {
        }

        public void Info(string message)
        {
        }
    }
}