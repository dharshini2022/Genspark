using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using PaymentProcessor.Models;
using System.Text.Json;
 
namespace PaymentProcessor.Controllers;
 
[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IProducer<string, string> _producer;
 
    private static readonly string[] DeclineReasons =
    {
        "Insufficient funds",
        "Card blocked by issuer",
        "Suspected fraud - unusual activity",
        "Daily transaction limit exceeded",
        "Card expired"
    };
 
    public PaymentController(IProducer<string, string> producer)
        => _producer = producer;
 
    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest req)
    {
        var rng      = new Random();
        var approved = rng.Next(0, 10) > 2;   // ~70% approval rate
 
        var result = new PaymentResult
        {
            TransactionId = req.TransactionId,
            MerchantName  = req.MerchantName,
            Amount        = req.Amount,
            CardLastFour  = req.CardLastFour,
            Status        = approved ? "APPROVED" : "DECLINED",
            Reason        = approved ? "Payment authorised"
                                     : DeclineReasons[rng.Next(DeclineReasons.Length)],
            Timestamp     = DateTimeOffset.UtcNow
        };
 
        await _producer.ProduceAsync("payment-events",
            new Message<string, string>
            {
                Key   = result.TransactionId,
                Value = JsonSerializer.Serialize(result)
            });
 
        return Ok(result);
    }
 
    [HttpGet("health")]
    public IActionResult Health()
        => Ok(new { status = "OK", time = DateTimeOffset.UtcNow });
}
