namespace PaymentProcessor.Models;
 
public class PaymentRequest
{
    public string TransactionId  { get; set; } = string.Empty;
    public string MerchantName   { get; set; } = string.Empty;
    public decimal Amount        { get; set; }
    public string CardLastFour   { get; set; } = string.Empty;
}
