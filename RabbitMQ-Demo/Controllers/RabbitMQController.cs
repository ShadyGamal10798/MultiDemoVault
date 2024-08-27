using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ_Demo.Model;

[ApiController]
[Route("[controller]")]
public class RabbitMQController : ControllerBase
{
    private readonly RabbitMQService _rabbitMQService;

    public RabbitMQController(RabbitMQService rabbitMQService)
    {
        _rabbitMQService = rabbitMQService;
    }

    [HttpPost("create-exchange")]
    public IActionResult CreateExchange(string exchangeName, string type = "direct")
    {
        try
        {
            _rabbitMQService.CreateExchange(exchangeName, type);
            return Ok($"Exchange {exchangeName} created with type {type}.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("check-exchange")]
    public IActionResult CheckExchange(string exchangeName)
    {
        var exists = _rabbitMQService.CheckExchangeExists(exchangeName);
        return Ok($"Exchange {exchangeName} exists: {exists}.");
    }


    [HttpPost("bind-exchange")]
    public IActionResult BindExchangeToQueue(string exchangeName, string routingKey)
    {
        const string queueName = "info_logs";  // The name of the queue
        try
        {
            _rabbitMQService.BindExchangeToQueue(exchangeName, queueName, routingKey);
            return Ok($"Exchange '{exchangeName}' bound to queue '{queueName}' with routing key '{routingKey}'.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error binding exchange to queue: {ex.Message}");
        }
    }

    [HttpPost("send-message")]
    public IActionResult SendMessage(string exchangeName, string routingKey, [FromBody] string message)
    {
        try
        {
            _rabbitMQService.PublishMessage(exchangeName, routingKey, message);
            return Ok($"Message sent to exchange '{exchangeName}' with routing key '{routingKey}'.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error sending message: {ex.Message}");
        }
    }


    [HttpPost("send-device-info")]
    public IActionResult SendDeviceInfo(
        [FromBody] DeviceInfo deviceInfo,
        [FromQuery] string exchangeName,
        [FromQuery] string routingKey)
    {
        try
        {
            var message = JsonConvert.SerializeObject(deviceInfo);
            _rabbitMQService.PublishMessage(exchangeName, routingKey, message);
            return Ok($"Message sent to {exchangeName} using {routingKey} successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error sending message: {ex.Message}");
        }
    }

}
