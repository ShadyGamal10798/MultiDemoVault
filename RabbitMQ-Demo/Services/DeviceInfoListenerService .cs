using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using RabbitMQ_Demo.Model;
using System.Text;
using Newtonsoft.Json;

public class DeviceInfoListenerService : BackgroundService
{
    private IModel _channel;
    private IConnection _connection;
    private readonly string _hostname;
    private readonly string _username;
    private readonly string _password;

    public DeviceInfoListenerService(IOptions<RabbitMQOptions> options)
    {
        _hostname = options.Value.Hostname;
        _username = options.Value.Username;
        _password = options.Value.Password;
        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _hostname,
            UserName = _username,
            Password = _password
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
            var deviceInfo = JsonConvert.DeserializeObject<DeviceInfo>(message);
            ProcessMessage(deviceInfo);
        };

        _channel.BasicConsume(queue: "shady2", autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }

    private void ProcessMessage(DeviceInfo deviceInfo)
    {
        Console.WriteLine($"Received AndroidId: {deviceInfo.AndroidId}, TenantId: {deviceInfo.TenantId}");
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
