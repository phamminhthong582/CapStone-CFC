using BusinessObject.DTO.Product;
using BusinessObject.DTO.ProductImages;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudinaryService _cloudinaryService;

        public ProductService(IUnitOfWork unitOfWork, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
        }

        public async Task CreateProduct(ProductRequest productRequest)
        {
            var product = new Product
            {
                ProductName = productRequest.ProductName,
                Quantity = productRequest.Quantity,
                Price = productRequest.Price,
                Size = productRequest.Size,
                Discount = productRequest.Discount,
                Weight = productRequest.Weight,
                Description = productRequest.Description,
                Featured = productRequest.Featured,
                CategoryId = productRequest.CategoryId,
                Sold = 0,
                Status = true,
                CreateAt = DateTime.Now,
            };
            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.CompleteAsync();

            var folderName = $"Product/{productRequest.ProductName}";

            if (productRequest.Images != null && productRequest.Images.Any())
            {
                Console.WriteLine($"Bắt đầu xử lý {productRequest.Images.Count()} ảnh");
                var productImages = new List<ProductImage>();

                int imageIndex = 0;
                foreach (var image in productRequest.Images)
                {
                    imageIndex++;
                    Console.WriteLine($"Đang xử lý ảnh thứ {imageIndex}");

                    // Tạo tên file duy nhất cho mỗi ảnh
                    var uniqueFileName = $"{folderName}/{Guid.NewGuid()}";
                    var imageUrl = await _cloudinaryService.UploadImageAsync(image.OpenReadStream(), uniqueFileName);

                    Console.WriteLine($"URL ảnh {imageIndex}: {imageUrl}");

                    var productImage = new ProductImage
                    {
                        ProductImage1 = imageUrl,
                        ProductId = product.ProductId,
                        CreateAt = DateTime.Now,
                        Status = true
                    };

                    productImages.Add(productImage);
                    Console.WriteLine($"Đã thêm ảnh {imageIndex} vào list");
                }

                Console.WriteLine($"Tổng số ảnh trong list: {productImages.Count}");
                await _unitOfWork.Repository<ProductImage>().AddRangeAsync(productImages);
                await _unitOfWork.CompleteAsync();
                Console.WriteLine("Đã lưu vào database");
            }
        }

        public async Task DeleteProduct(Guid ProductId)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException("product not found");
            }
            _unitOfWork.Repository<Product>().Delete(product);
            var allImages = await _unitOfWork.Repository<ProductImage>().GetAllAsync();
            var ProductImages = allImages.Where(image => image.ProductId == product.ProductId);
            _unitOfWork.Repository<ProductImage>().DeleteRange(ProductImages);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<ProductResponse> GetProductById(Guid id)
        {
            var product = await _unitOfWork.Repository<Product>().Entities
                        .Include(p => p.Category)
                        .FirstOrDefaultAsync(p => p.ProductId == id);

            var allImages = await _unitOfWork.Repository<ProductImage>().GetAllAsync();

            var productResponse = new ProductResponse{
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Quantity = product.Quantity,
                Price = product.Price,
                Weight = product.Weight,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
              
                Size = product.Size,
                Discount = product.Discount,
                Description = product.Description,
                Featured = product.Featured,
                CategoryName = product.Category?.CategoryName,
                Sold = product.Sold,
                Status = product.Status,
                ProductImages = allImages.Where(image => image.ProductId == product.ProductId)
                                         .Select(image => new ProductImagesResponse
                                         {
                                             ProductImageId = image.ProductImageId,
                                             ProductImage1 = image.ProductImage1,
                                             CreateAt = image.CreateAt,
                                             UpdateAt = image.UpdateAt,
                                             Status = image.Status,
                                         }).ToList()
            };

            return productResponse;
        }

    
        
        public async Task<IEnumerable<ProductResponse>> GetProducts()
        {
            var products = await _unitOfWork.Repository<Product>().Entities.Include(n => n.Category).ToListAsync();
            var allImages = await _unitOfWork.Repository<ProductImage>().GetAllAsync();

            var productResponse = products.Select(product => new ProductResponse
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Price = product.Price,
                Quantity = product.Quantity,
                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
                Weight = product.Weight,
                Size = product.Size,
                Discount = product.Discount,
                Description = product.Description,
                Featured = product.Featured,
                CategoryName = product.Category?.CategoryName,
                Sold = product.Sold,
                Status = product.Status,
                ProductImages = allImages.Where(image => image.ProductId == product.ProductId)
                                         .Select(image => new ProductImagesResponse
                                         {
                                             ProductImageId = image.ProductImageId,
                                             ProductImage1 = image.ProductImage1,
                                             CreateAt = image.CreateAt,
                                             UpdateAt = image.UpdateAt,
                                             Status = image.Status,
                                         }).ToList()
            });

            return productResponse;
        }

    /*    public async Task<IEnumerable<ProductResponse>> GetProductsByStoreId(Guid StoreId)
        {
            var products = (await _unitOfWork.Repository<Product>().GetAllAsync()).Where(product => product.StoreId == StoreId);

            var allImages = await _unitOfWork.Repository<ProductImage>().GetAllAsync();

            var productResponse = products.Select(product => new ProductResponse
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                StoreId = product.StoreId,
                Quantity = product.Quantity,
                Price = product.Price,

                CreateAt = DateTime.Now,
                UpdateAt = DateTime.Now,
                Size = product.Size,
                Discount = product.Discount,
                Description = product.Description,
                Featured = product.Featured,
                CategoryName = product.Category?.CategoryName,
                Sold = product.Sold,
                Status = product.Status,
                ProductImages = allImages.Where(image => image.ProductId == product.ProductId)
                                         .Select(image => new ProductImagesResponse
                                         {
                                             ProductImageId = image.ProductImageId,
                                             ProductImage1 = image.ProductImage1,
                                             CreateAt = image.CreateAt,
                                             UpdateAt = image.UpdateAt,
                                             Status = image.Status,
                                         }).ToList()
            });

            return productResponse;
        }*/

        public async Task UpdateProduct(UpdateProductRequest updateProductRequest, Guid ProductId)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }
            product.ProductName = updateProductRequest.ProductName ?? product.ProductName; 
            product.Quantity = updateProductRequest.Quantity?? product.Quantity;
            product.Description = updateProductRequest.Description ?? product.Description;
            product.Price= updateProductRequest.Price?? product.Price;
            product.Size = updateProductRequest.Size ?? product.Size;
            product.Weight = updateProductRequest.Weight ?? product.Weight;
            product.Discount = updateProductRequest.Discount ?? product.Discount;
            product.Featured = updateProductRequest.Featured ?? product.Featured;
            product.CategoryId = updateProductRequest.CategoryId ?? product.CategoryId;
            product.Status = updateProductRequest.Status ?? product.Status;
            product.UpdateAt = DateTime.Now;
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();
        }
    }
}
