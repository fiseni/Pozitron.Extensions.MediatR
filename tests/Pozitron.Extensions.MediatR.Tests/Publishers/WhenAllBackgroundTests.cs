using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Tests;

public class WhenAllBackgroundTests
{
    private readonly TestQueue<int> _queue = new();

    public record Ping() : INotification { }

    public class Pong1(TestQueue<int> queue) : INotificationHandler<Ping>
    {
        public const int Id = 1;
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            // Adding delay at the start just so we're sure won't be the first message.
            await Task.Delay(10, cancellationToken);
            queue.Write(Id);
            await Task.Delay(200, cancellationToken);
        }
    }
    public class Pong2() : INotificationHandler<Ping>
    {
        public Task Handle(Ping notification, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
    public class Pong3() : INotificationHandler<Ping>
    {
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken);
            throw new AggregateException(new NotSupportedException(), new Exception());
        }
    }
    public class Pong4() : INotificationHandler<Ping>
    {
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            await Task.Delay(20, cancellationToken);
            throw new TaskCanceledException();
        }
    }
    public class Pong5(TestQueue<int> queue) : INotificationHandler<Ping>
    {
        public const int Id = 5;
        public async Task Handle(Ping notification, CancellationToken cancellationToken)
        {
            // Adding delay at the start just so we're sure won't be the first message.
            await Task.Delay(30, cancellationToken);
            queue.Write(Id);
            await Task.Delay(200, cancellationToken);
        }
    }

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
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Ping(), PublishStrategy.WhenAllBackground);
        _queue.Write(0);
        await _queue.WaitForCompletion(expectedMessages: 3, timeoutInMilliseconds: 300);

        var result = _queue.GetValues();
        result.Should().HaveCount(3);
        result[0].Should().Be(0);
        result[1].Should().Be(Pong1.Id);
        result[2].Should().Be(Pong5.Id);
    }

    [Fact]
    public async Task FlattensExceptions()
    {
        var logger = new FakeLogger<ExtendedMediator>();
        var services = new ServiceCollection();
        services.AddSingleton(_queue);
        services.AddSingleton(typeof(ILogger<ExtendedMediator>), _ => logger);
        services.AddExtendedMediatR(typeof(Ping));
        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Publish(new Ping(), PublishStrategy.WhenAllBackground);
        _queue.Write(0);
        await Task.Delay(300);

        logger.Exception.Should().NotBeNull();
        logger.Exception.Should().BeOfType<AggregateException>();
        var aggregateException = (AggregateException)logger.Exception!;
        aggregateException.InnerExceptions.Should().HaveCount(4);
        aggregateException.InnerExceptions[0].Should().BeOfType<NotImplementedException>();
        aggregateException.InnerExceptions[1].Should().BeOfType<NotSupportedException>();
        aggregateException.InnerExceptions[2].Should().BeOfType<Exception>();
        aggregateException.InnerExceptions[3].Should().BeOfType<TaskCanceledException>();

        var result = _queue.GetValues();
        result.Should().HaveCount(3);
        result[0].Should().Be(0);
        result[1].Should().Be(Pong1.Id);
        result[2].Should().Be(Pong5.Id);
    }
}
