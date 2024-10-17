namespace Tests;

public class FakeLogger<T> : ILogger<T>
{
    private readonly TestQueue<int> _testQueue;

    public FakeLogger(TestQueue<int> testQueue)
    {
        _testQueue = testQueue;
    }

    public Exception? Exception { get; private set; }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => throw new NotImplementedException();
    public bool IsEnabled(LogLevel logLevel) => throw new NotImplementedException();
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Exception = exception;

        if (exception is AggregateException aggregateException)
        {
            foreach (var innerException in aggregateException.InnerExceptions)
            {
                if (int.TryParse(innerException.Message, out var message))
                {
                    _testQueue.Write(message);
                }
            }
        }
        else if (exception is not null)
        {
            if (int.TryParse(exception.Message, out var message))
            {
                _testQueue.Write(message);
            }
        }
    }
}
