using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Tests;

public class TestQueue<T>
{
    private readonly ConcurrentQueue<T> _output = new();

    public T[] GetValues() => _output.ToArray();

    public void Write(T value)
    {
        _output.Enqueue(value);
    }

    public async Task WaitForCompletion(int expectedMessages, int timeoutInMilliseconds)
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(timeoutInMilliseconds);

        while (!cts.Token.IsCancellationRequested && _output.Count < expectedMessages)
        {
            await Task.Delay(10, CancellationToken.None);
        }
    }
}

public class FakeLogger<T> : ILogger<T>
{
    public Exception? Exception { get; private set; }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => throw new NotImplementedException();
    public bool IsEnabled(LogLevel logLevel) => throw new NotImplementedException();
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Exception = exception;
    }
}
