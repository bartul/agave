using Agave;
using Agave.Tests.TestExtensions;
using Microsoft.Extensions.Logging;

namespace Agave.Tests;

public class AgaveTests
{
    private readonly LoggerFactory _loggerFactory;

    public AgaveTests(ITestOutputHelper output)
    {
        _loggerFactory = new LoggerFactory([new XunitLoggerProvider(output)]);  
    }

    [Fact]
    public async void WhenAgaveIsPlanted_ThenDoNothing()
    {
        var agaveId = Guid.NewGuid();   
        IAgave agave = new Agave(_loggerFactory, () => agaveId);
        await agave.Plant();
    }
}

