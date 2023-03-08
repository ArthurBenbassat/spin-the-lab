using Microsoft.Extensions.Logging;

namespace Home.shared
{
    internal class SpinLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.Debug)
                return false;
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;
            if (null != exception)
            {
                Console.Error.WriteLine($"  {formatter(state, exception)}");
                Console.Error.WriteLine(exception.ToString());
            }
            else
            {
                Console.WriteLine($"  {formatter(state, exception)}");
            }
        }
    }
}