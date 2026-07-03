namespace PaymentProcessor.Models;
 
public class PaymentResult
{
    public string TransactionId  { get; set; } = string.Empty;
    public string MerchantName   { get; set; } = string.Empty;
    public decimal Amount        { get; set; }
    public string CardLastFour   { get; set; } = string.Empty;
    public string Status         { get; set; } = string.Empty;
    public string Reason         { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}
