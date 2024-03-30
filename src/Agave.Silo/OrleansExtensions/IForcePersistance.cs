using System.Reflection;
using Orleans.Concurrency;

namespace Agave.OrleansExtensions;

public interface IForcePersistance : IIncomingGrainCallFilter
{
    async Task IIncomingGrainCallFilter.Invoke(IIncomingGrainCallContext context)
    {
        await context.Invoke();

        if(context.ImplementationMethod.GetCustomAttribute<ReadOnlyAttribute>() is null)
        {
            await WriteState();
        }
    }
    Task WriteState();
}
