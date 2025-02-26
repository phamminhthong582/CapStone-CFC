using AutoMapper;
using BusinessObject.DTO.Accessory;
using BusinessObject.DTO.Request;
using BusinessObject.DTO.Style;
using BusinessObject.Entities;
using Repository.Interface;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class AccessoryService : IAccessoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly CloudinaryService _cloudinaryService;

        public AccessoryService(IUnitOfWork unitOfWork, IMapper mapper, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
        }
        public async Task CreateAccessory(AccessoryRequest accessoryRequest)
        {
            var folderName = $"flowerBasket/{accessoryRequest.Name}";
            var AccessoryUrl = accessoryRequest.Image != null
            ? await _cloudinaryService.UploadImageAsync(accessoryRequest.Image.OpenReadStream(), $"{folderName}")
            : null;
            var newAccessory = new Accessory
            {
                Name = accessoryRequest.Name,
                Description = accessoryRequest.Description,
                Note = accessoryRequest.Note,
                Price = accessoryRequest.Price,
                Image = AccessoryUrl,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                Status = true,

            };
            await _unitOfWork.GetRepo<Accessory>().AddAsync(newAccessory);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteAccessory(Guid id)
        {
            var accessory = await _unitOfWork.GetRepo<Accessory>().GetByIdAsync(id);
            _unitOfWork.GetRepo<Accessory>().Delete(accessory);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<AccessoryResponse> GetAccessoryById(Guid id)
        {
            var style = await _unitOfWork.GetRepo<Accessory>().GetByIdAsync(id);
            return _mapper.Map<AccessoryResponse>(style);
        }

        public async Task<IEnumerable<AccessoryResponse>> GetAllAccessory()
        {
            var list = await _unitOfWork.GetRepo<Accessory>().GetAllAsync();
            return _mapper.Map<IEnumerable<AccessoryResponse>>(list);
        }

        public async Task UpdateAccessory(Guid id, AccessoryRequest accessoryRequest)
        {
            var asccessory = await _unitOfWork.GetRepo<Accessory>().GetByIdAsync(id);
            if (asccessory == null)
            {
                throw new KeyNotFoundException($"Style with ID {id} not found.");
            }
            var folderName = $"flowerBasket/{accessoryRequest.Name}";
            var AccessoryUrl = accessoryRequest.Image != null
            ? await _cloudinaryService.UploadImageAsync(accessoryRequest.Image.OpenReadStream(), $"{folderName}")
            : null;
            asccessory.Name = !string.IsNullOrEmpty(accessoryRequest.Name) ? accessoryRequest.Name : asccessory.Name;
            asccessory.Description = !string.IsNullOrEmpty(accessoryRequest.Description) ? accessoryRequest.Description : asccessory.Description;
            asccessory.Note = !string.IsNullOrEmpty(accessoryRequest.Note) ? accessoryRequest.Note : asccessory.Note;
            if (accessoryRequest.Price.HasValue && accessoryRequest.Price > 0)
            {
                asccessory.Price = accessoryRequest.Price.Value;
            }
            asccessory.Status = accessoryRequest.Status ?? asccessory.Status;
            asccessory.Image = AccessoryUrl ?? asccessory.Image;
            asccessory.UpdateAt = DateTime.Now;

            _unitOfWork.GetRepo<Accessory>().Update(asccessory);
            await _unitOfWork.CompleteAsync();
        }
    }
}
