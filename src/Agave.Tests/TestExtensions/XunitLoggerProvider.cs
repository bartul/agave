using Microsoft.Extensions.Logging;

namespace Agave.Tests.TestExtensions
{
    public class XunitLoggerProvider(ITestOutputHelper output) : ILoggerProvider
    {
        private readonly ITestOutputHelper output = output;

        public ILogger CreateLogger(string categoryName) => new XunitLogger(this.output, categoryName);

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private class XunitLogger : ILogger, IDisposable
        {
            private readonly ITestOutputHelper output;
            private readonly string category;

            public XunitLogger(ITestOutputHelper output, string category)
            {
                this.output = output;
                this.category = category;
            }
            
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => this;

            public void Dispose() { }

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
            {
                this.output.WriteLine($"{logLevel} [{this.category}.{eventId.Name ?? eventId.Id.ToString()}] {formatter(state, exception ?? new Exception())}");
            }
        }
    }
}
