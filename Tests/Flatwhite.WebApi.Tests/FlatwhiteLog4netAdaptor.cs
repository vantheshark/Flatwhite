using System;
using log4net;

namespace Flatwhite.WebApi.Tests
{
    public class FlatwhiteLog4netAdaptor : ILogger
    {
        private readonly ILog _logger;

        public FlatwhiteLog4netAdaptor(ILog logger)
        {
            _logger = logger;
        }

        public void Error(Exception ex)
        {
            _logger.Error(ex);
        }

        public void Error(string message, Exception ex)
        {
            _logger.Error(message, ex);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }
    }
}