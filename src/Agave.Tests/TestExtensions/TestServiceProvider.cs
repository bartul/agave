using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.BroadcastChannel;

namespace Agave.Tests.TestExtensions;

public class TestServiceProvider(ILogger logger) : IKeyedServiceProvider
{
    private readonly ILogger _logger = logger;
    private readonly Dictionary<(Type, object), object> _keyedServices = [];

    public void AddKeyedService(Type serviceType, object serviceKey, object service)
    {
        _keyedServices.Add((serviceType, serviceKey), service);
    }
    public void AddKeyedService<T>(object serviceKey, object service)
    {
        _keyedServices.Add((typeof(T), serviceKey), service);
    }
    public object? GetKeyedService(Type serviceType, object? serviceKey)
    {
        if(serviceKey is null) return GetService(serviceType);
        return _keyedServices.TryGetValue((serviceType, serviceKey), out var service) ? service : null;

    }

    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
    {
        return GetKeyedService(serviceType, serviceKey) ?? throw new KeyNotFoundException();
    }

    public object? GetService(Type serviceType) => serviceType switch
    {
        Type t when t == typeof(IBroadcastChannelProvider) => new TestBroadcastChannelProvider(_logger),
        _ => null,
    };
}