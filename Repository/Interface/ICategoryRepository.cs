using BusinessObject.Entities;

namespace Repository.Interface;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllCategory();
    Task<List<Category>> GetAllCategoryByProductType();
    Task<List<Category>> GetAllCategoryByFlowerType();
    Task<List<Category>> GetAllCategoryByBasketType();

    Task<Category> GetCategoryById(Guid id);
    Task<Category> AddCategory(Category category);
    Task<Category> UpdateCategory(Category category);
    Task<Category> DeleteCategory(Guid id);
   

}