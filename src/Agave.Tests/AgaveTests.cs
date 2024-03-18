using Agave;
using Agave.Tests.TestExtensions;
using Microsoft.Extensions.Logging;

namespace Agave.Tests;

public partial class AgaveTests
{
    private readonly LoggerFactory _loggerFactory;

    public AgaveTests(ITestOutputHelper output)
    {
        _loggerFactory = new LoggerFactory([new XunitLoggerProvider(output)]);  
    }

    [Fact]
    public async void WhenPlantCommandIsExecuted_ThenAgaveIsPlanted()
    {
        var agaveId = Guid.NewGuid();
        var state = new AgaveState();
        IAgave agave = new Agave(new TestPersistentState<AgaveState>(state), _loggerFactory, () => agaveId);
        await agave.Plant();

        Assert.Equal(AgaveBlossomState.Planted, state.Current);
    }
}

