using Orleans.Core;
using Orleans.Runtime;

namespace Agave.Tests.TestExtensions;

internal class TestPersistentState<T> : IPersistentState<T>
{
    public TestPersistentState(T state, string? eTag = null)
    {
        ((IStorage<T>)this).State = state;
        ETag = eTag ?? Guid.NewGuid().ToString(); 
    }

    T IStorage<T>.State { get; set; } = default!;
    public string ETag { get; set; } = default!;

    string IStorage.Etag => throw new NotImplementedException();
    bool IStorage.RecordExists => throw new NotImplementedException();
    Task IStorage.ClearStateAsync() => throw new NotImplementedException();
    Task IStorage.WriteStateAsync() => throw new NotImplementedException();
    Task IStorage.ReadStateAsync() => throw new NotImplementedException();
}

