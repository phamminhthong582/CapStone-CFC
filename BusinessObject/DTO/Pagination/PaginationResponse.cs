namespace BusinessObject.DTO.Pagination;

public class PaginationResponse<T>
{
    // Tổng số bản ghi
    public int TotalCount { get; set; }

    // Số trang hiện tại
    public int CurrentPage { get; set; }

    // Tổng số trang
    public int TotalPages { get; set; }

    // Kích thước mỗi trang
    public int PageSize { get; set; }

    // Dữ liệu của trang hiện tại
    public IEnumerable<T> Data { get; set; }

    // Constructor mặc định
    public PaginationResponse()
    {
        Data = new List<T>();
    }

    // Constructor để khởi tạo các giá trị
    public PaginationResponse(int totalCount, int currentPage, int pageSize, IEnumerable<T> data)
    {
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
        Data = data;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}