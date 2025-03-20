namespace BusinessObject.DTO.Pagination;

public class PaginationResponse<T>
{
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<T> Data { get; set; }
    public PaginationResponse()
    {
        Data = new List<T>();
    }
    public PaginationResponse(int totalCount, int currentPage, int pageSize, IEnumerable<T> data)
    {
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
        Data = data;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}