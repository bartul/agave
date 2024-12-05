using Microsoft.Extensions.Logging;

namespace Agave.Tests.TestExtensions;

internal class TestGrainContext<T> : IGrainContext
{
    private readonly GrainId _grainId = GrainId.Create(typeof(T).Name, Guid.NewGuid().ToString());
    
    private readonly ILogger _logger;
    private readonly TestServiceProvider _testServiceProvider;

    public TestGrainContext(ILogger logger, TestServiceProvider testServiceProvider)
    {
        _logger = logger;
        _testServiceProvider = testServiceProvider;
    }
    public TestGrainContext(ILogger logger)
    {
        _logger = logger;
        _testServiceProvider = new TestServiceProvider(logger);
    }

    public GrainReference GrainReference => throw new NotImplementedException();

    public GrainId GrainId => _grainId;

    public object GrainInstance => throw new NotImplementedException();

    public ActivationId ActivationId => throw new NotImplementedException();

    public GrainAddress Address => throw new NotImplementedException();

    public IServiceProvider ActivationServices => _testServiceProvider;

    public IGrainLifecycle ObservableLifecycle => throw new NotImplementedException();

    public IWorkItemScheduler Scheduler => throw new NotImplementedException();

    public Task Deactivated => throw new NotImplementedException();

    public void Activate(Dictionary<string, object> requestContext, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public void Deactivate(DeactivationReason deactivationReason, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public bool Equals(IGrainContext? other)
    {
        throw new NotImplementedException();
    }

    public TComponent GetComponent<TComponent>() where TComponent : class
    {
        throw new NotImplementedException();
    }

    public TTarget GetTarget<TTarget>() where TTarget : class
    {
        throw new NotImplementedException();
    }

    public void Migrate(Dictionary<string, object> requestContext, CancellationToken? cancellationToken = null)
    {
        throw new NotImplementedException();
    }

    public void ReceiveMessage(object message)
    {
        throw new NotImplementedException();
    }

    public void Rehydrate(IRehydrationContext context)
    {
        throw new NotImplementedException();
    }

    public void SetComponent<TComponent>(TComponent? value) where TComponent : class
    {
        throw new NotImplementedException();
    }

    public void Activate(Dictionary<string, object>? requestContext, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Deactivate(DeactivationReason deactivationReason, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Migrate(Dictionary<string, object>? requestContext, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}