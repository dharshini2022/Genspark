namespace BankingAPI.Models.DTOs
{
    public class FilterRequest
    {
        public string AccountNumber {get; set;} = string.Empty;
        public int PageNumber {get; set;} = 1;
        public int PageSize {get; set;} = 3;
        public DateTime? FromDate {get; set;}
        public DateTime? ToDate {get; set;}

        public float? MinAmount {get; set;}
        public float? MaxAmount {get; set;}
    }
}