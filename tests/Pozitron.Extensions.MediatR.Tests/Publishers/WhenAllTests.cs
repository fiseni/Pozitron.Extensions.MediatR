namespace Tests;

public class WhenAllTests
{
    private readonly TestQueue<int> _queue = new();

    [Fact]
    public async Task ExecutesAllHandlersConcurrently()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<Ping>();
            x.TypeEvaluator = type => type.Name is (nameof(Pong1) or nameof(Pong5));
        });
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Ping(), PublishStrategy.WhenAll);

        _queue.Write(0);
        await _queue.WaitForCompletion(expectedMessages: 3, timeoutInMilliseconds: 300);

        var result = _queue.GetValues();
        result.Should().Equal(Pong1.Id, Pong5.Id, 0);
    }

    [Fact]
    public async Task FlattensExceptions()
    {
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddExtendedMediatR(typeof(Ping));
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var sut = async () => await mediator.Publish(new Ping(), PublishStrategy.WhenAll);
        var ex = await sut.Should().ThrowExactlyAsync<AggregateException>();

        _queue.Write(0);
        await _queue.WaitForCompletion(expectedMessages: 3, timeoutInMilliseconds: 300);

        ex.Which.InnerExceptions.Should().HaveCount(5);
        ex.Which.InnerExceptions[0].Should().BeOfType<NotImplementedException>();
        ex.Which.InnerExceptions[1].Should().BeOfType<NotSupportedException>();
        ex.Which.InnerExceptions[2].Should().BeOfType<Exception>();
        ex.Which.InnerExceptions[3].Should().BeOfType<Exception>();
        ex.Which.InnerExceptions[4].Should().BeOfType<TaskCanceledException>();

        var result = _queue.GetValues();
        result.Should().Equal(Pong1.Id, Pong5.Id, 0);
    }

    public record Ping() : INotification { }

    public class Pong1(TestQueue<int> queue) : INotificationHandler<Ping>
    {
        public const int Id = 1;
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken);
            queue.Write(Id);
            await Task.Delay(200, cancellationToken);
        }
    }
    public class Pong2() : INotificationHandler<Ping>
    {
        public Task Handle(Ping notification, CancellationToken cancellationToken)
            => throw new NotImplementedException("21");
    }
    public class Pong3() : INotificationHandler<Ping>
    {
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken);
            throw new AggregateException(
                new NotSupportedException("31"),
                new AggregateException(new Exception("32"), new Exception("33")));
        }
    }
    public class Pong4() : INotificationHandler<Ping>
    {
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Delay(30, cancellationToken);
            throw new TaskCanceledException("41");
        }
    }
    public class Pong5(TestQueue<int> queue) : INotificationHandler<Ping>
    {
        public const int Id = 5;
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Delay(30, cancellationToken);
            queue.Write(Id);
            await Task.Delay(200, cancellationToken);
        }
    }
}
