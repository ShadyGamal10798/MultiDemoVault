using RabbitMQ.Client;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Exceptions;
using RabbitMQ_Demo.Model;
using System.Text;

public class RabbitMQService : IDisposable
{
    private readonly IConnection connection;
    private readonly IModel channel;

    public RabbitMQService(IOptions<RabbitMQOptions> options)
    {
        var factory = new ConnectionFactory()
        {
            HostName = options.Value.Hostname,
            UserName = options.Value.Username,
            Password = options.Value.Password
        };
        this.connection = factory.CreateConnection();
        this.channel = connection.CreateModel();
    }

    public void CreateExchange(string exchangeName, string type = "direct")
    {
        channel.ExchangeDeclare(exchange: exchangeName, type: type, durable: true);
    }

    public bool CheckExchangeExists(string exchangeName)
    {
        try
        {
            // Attempt to declare the exchange passively (i.e., only check its existence)
            channel.ExchangeDeclarePassive(exchangeName);
            return true;  // The exchange exists
        }
        catch (OperationInterruptedException ex)
        {
            // Check if the reason for the exception is that the exchange does not exist
            if (ex.ShutdownReason.ReplyCode == 404)  // 404 is the code for 'Not Found'
            {
                return false;  // The exchange does not exist
            }
            else
            {
                // Rethrow or handle other operational issues differently
                throw;  // or log it and handle accordingly
            }
        }
        catch (Exception ex)
        {
            // Handle other exceptions that could occur (network issues, etc.)
            throw;  // or log it and decide how to respond
        }
    }

    public void BindExchangeToQueue(string exchangeName, string queueName, string routingKey)
    {
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
    }

    public void PublishMessage(string exchangeName, string routingKey, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: null, body: body);
    }


    public void Dispose()
    {
        this.channel?.Close();
        this.connection?.Close();
    }
}
