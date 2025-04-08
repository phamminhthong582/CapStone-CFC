using AutoMapper;
using BusinessObject.DTO.Comment;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Pagination;
using BusinessObject.Entities;
using Repository.Interface;
using Service.Interface;
using System.Net.Http.Json;
using System.Text.Json;

namespace Service.Implement;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IMapper _mapper;

    public CommentService(ICommentRepository commentRepository , IMapper mapper)
    {
        _commentRepository = commentRepository;
        _mapper = mapper;
    }
    public async Task<List<CommentResponse>> GetAllComment()
    {
        var list = await _commentRepository.GetAllComment();
        return _mapper.Map<List<CommentResponse>>(list);
    }

    public async Task<PaginationResponse<CommentResponse>> GetAllCommentPagination(int pageNumber, int pageSize)
    {
        var totalCount = await _commentRepository.CountCommentsAsync();  
        var comments = await _commentRepository.GetCommentPaginatedAsync(pageNumber, pageSize); 
        var commentsResponses = _mapper.Map<List<CommentResponse>>(comments);
        var paginationResponse = new PaginationResponse<CommentResponse>
        {
            TotalCount = totalCount,
            CurrentPage = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Data = commentsResponses
        };
        return paginationResponse;
    }
    public class ModerationResponse
    {
        public List<ModerationResult> Results { get; set; }
    }

    public class ModerationResult
    {
        public bool Flagged { get; set; }
    }

    public async Task<bool> IsInappropriateAsync(string content)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("Authorization", "Bearer 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");

        var data = new { input = content };
        var response = await http.PostAsJsonAsync("https://api.openai.com/v1/moderations", data);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Moderation API failed with status {response.StatusCode}: {responseContent}");
            return true; // fail-safe
        }

        try
        {
            var json = JsonDocument.Parse(responseContent);
            var result = json.RootElement.GetProperty("results")[0];

            bool flagged = result.GetProperty("flagged").GetBoolean();
            if (flagged)
            {
                Console.WriteLine("Content flagged by moderation.");
                return true;
            }

            var scores = result.GetProperty("category_scores");
            foreach (var category in scores.EnumerateObject())
            {
                decimal score = category.Value.GetDecimal();
                if (score > 0.8m) // tuỳ chỉnh ngưỡng nếu muốn nghiêm ngặt hơn
                {
                    Console.WriteLine($"Content has high score in '{category.Name}': {score}");
                    return true;
                }
            }

            return false; // không bị flag và không có điểm cao đáng nghi
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing moderation response: {ex.Message}");
            return true; // fail-safe nếu có lỗi
        }
    }

    public async Task<Result<CommentResponse>> CreateComment(CreateCommentRequest request)
    {
        var response = new Result<CommentResponse>();

        if (request.ProductId == null || request.CustomerId == null)
        {
            response.ResultStatus = ResultStatus.Error.ToString();
            response.Messages = new[] { "ProductId and CustomerId are required." };
            return response;
        }

        if (request.Rating == null || request.Rating < 1 || request.Rating > 5)
        {
            response.ResultStatus = ResultStatus.Error.ToString();
            response.Messages = new[] { "Rating must be between 1 and 5." };
            return response;
        }

        if (!string.IsNullOrWhiteSpace(request.Feedback))
        {
            bool isInappropriate = await IsInappropriateAsync(request.Feedback);
            if (isInappropriate)
            {
                response.ResultStatus = ResultStatus.Error.ToString();
                response.Messages = new[] { "Feedback contains inappropriate or harmful language." };
                return response;
            }
        }

        var comment = new Comment
        {
            ProductId = request.ProductId.Value,
            CustomerId = request.CustomerId.Value,
            Rating = request.Rating.Value,
            Feedback = request.Feedback ?? string.Empty,
            Status = request.Status ?? true,
        };

        await _commentRepository.AddComment(comment);

        response.ResultStatus = ResultStatus.Success.ToString();
        response.Messages = new[] { "Comment created successfully." };
        response.Data = new CommentResponse
        {
            CommentId = comment.CommentId,
            ProductId = comment.ProductId,
            CustomerId = comment.CustomerId,
            Rating = comment.Rating,
            Feedback = comment.Feedback,
            Status = comment.Status,
        };

        return response;
    }

    public async Task<List<CommentResponse>> GetCommentByProductId(Guid productId)
    {
        var comments = await _commentRepository.GetCommentByProductId(productId);

        return comments.Select(c => new CommentResponse
        {
            CommentId = c.CommentId,
            Feedback = c.Feedback,
            CustomerId = c.CustomerId,
            Rating = c.Rating,
            Status = c.Status,
            CustomerName = c.Customer != null ? c.Customer.Email : "Anonymous", 
        }).ToList();

    }
}