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

    public async Task<T[]> WaitForCompletion(int expectedMessages, int timeoutInMilliseconds)
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(timeoutInMilliseconds);

        while (!cts.Token.IsCancellationRequested && _output.Count < expectedMessages)
        {
            await Task.Delay(10, CancellationToken.None);
        }

        return _output.ToArray();
    }
}
