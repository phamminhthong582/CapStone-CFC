using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryService(IConfiguration configuration)
        {
            var cloudName = configuration["CloudinarySettings:CloudName"];
            var apiKey = configuration["CloudinarySettings:ApiKey"];
            var apiSecret = configuration["CloudinarySettings:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }
        /*  public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
          {
              var uploadParams = new ImageUploadParams()
              {
                  File = new FileDescription(fileName, fileStream),
                  PublicId = $"my_images/{fileName}", // Tùy chỉnh đường dẫn lưu trên Cloudinary
                  Overwrite = true
              };

              var uploadResult = await _cloudinary.UploadAsync(uploadParams);
              return uploadResult.SecureUrl.ToString(); // Trả về URL của ảnh đã upload
          }*/
        public async Task<string> UploadImageAsync(Stream fileStream, string folderPath)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription("image", fileStream),
                Folder = folderPath, // Đặt thư mục lưu trữ trên Cloudinary
                UseFilename = true,  // Sử dụng tên file gốc
                UniqueFilename = false, // Không tự động đổi tên
                Overwrite = true // Ghi đè nếu file trùng
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl.ToString();
        }
        public async Task<string?> UploadVideoAsync(Stream videoStream, string fileName)
        {
            var uploadParams = new VideoUploadParams()
            {
                File = new FileDescription(fileName, videoStream),
                PublicId = $"flowerBasket/videos/{fileName}",
                Folder = "flowerBasket/videos",
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadLargeAsync(uploadParams);

            return uploadResult.SecureUrl?.ToString();
        }

    }
}
