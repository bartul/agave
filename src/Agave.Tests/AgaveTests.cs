using Agave;
using Microsoft.Extensions.Logging;
namespace Agave.Tests;

public class AgaveTests
{
    [Fact]
    public async void WhenAgaveIsPlanted_ThenPublishAgavePlantedEvent()
    {
        var agaveId = Guid.NewGuid();   
        IAgave agave = new Agave(new TestLoggerFactory(), () => agaveId);
        await agave.Plant();
    }
}

internal class TestLoggerFactory : ILoggerFactory
{
    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger();
    }

    public void AddProvider(ILoggerProvider provider)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

internal class TestLogger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine($"Log: {logLevel} - {formatter(state, exception)}");
    }
}