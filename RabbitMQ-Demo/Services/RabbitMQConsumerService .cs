using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ_Demo.Model;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class RabbitMQConsumerService : BackgroundService
{
    private readonly RabbitMQOptions _options;
    private IConnection _connection;
    private IModel _channel;

    public RabbitMQConsumerService(IOptions<RabbitMQOptions> options)
    {
        _options = options.Value;
        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _options.Hostname,
            UserName = _options.Username,
            Password = _options.Password
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            ProcessMessage(message);
        };

        _channel.BasicConsume(queue: "info_logs", autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }

    private void ProcessMessage(string message)
    {
        Console.WriteLine($"Received message: {message}");
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
