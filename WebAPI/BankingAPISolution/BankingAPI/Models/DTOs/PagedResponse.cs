namespace BankingAPI.Models.DTOs
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalRecords { get; set; }

        public int TotalPages { get; set; }

        public bool HasNextPage => PageNumber < TotalPages;

        public bool HasPreviousPage => PageNumber > 1;
    }
}