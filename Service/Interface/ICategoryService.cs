using BusinessObject.DTO.Category;
using BusinessObject.DTO.Commons;
using BusinessObject.DTO.Pagination;
using BusinessObject.Entities;

namespace Service.Interface;

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllCategory();
    Task<List<CategoryResponse>> GetCategoryByProductType();
    Task<List<CategoryResponse>> GetCategoryByFlowerType();
    Task<List<CategoryResponse>> GetCategoryByBasketType();
    Task<List<CategoryResponse>> GetCategoryByStyleType();
    Task<List<CategoryResponse>> GetCategoryByAccessoryType();
    Task<PaginationResponse<CategoryResponse>> GetAllCategoryPagination(int pageNumber, int pageSize);

    Task<Result<Category>> CreateCategory(CreateCategoryRequest request);
    Task<Result<CategoryResponse>> UpdateNameCategory(Guid id,UpdateCategoryRequest request);
    Task<Result<Category>> DeleteCategory(Guid id);
    Task<Result<CategoryResponse>> GetCategoryById(Guid id);

}