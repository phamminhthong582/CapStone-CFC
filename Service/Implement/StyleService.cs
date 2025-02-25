using AutoMapper;
using BusinessObject.DTO.Style;
using BusinessObject.Entities;
using Repository.Implement;
using Repository.Interface;
using Service.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class StyleService : IStyleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly CloudinaryService _cloudinaryService;

        public StyleService(IUnitOfWork unitOfWork, IMapper mapper, CloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
        }

        public async Task CreateStyle(StyleRequest styleRequest)
        {
            var folderName = $"flowerBasket/{styleRequest.Name}";
            var StyleUrl = styleRequest.Image != null
            ? await _cloudinaryService.UploadImageAsync(styleRequest.Image.OpenReadStream(), $"{folderName}")
            : null;
            var newStyle = new Style
            {
                Name = styleRequest.Name,
                Description = styleRequest.Description,
                Note = styleRequest.Note,
                Image = StyleUrl,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                Status = true,

            };
            await _unitOfWork.GetRepo<Style>().AddAsync(newStyle);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteStyle(Guid id)
        {
            var style = await _unitOfWork.GetRepo<Style>().GetByIdAsync(id);
            _unitOfWork.GetRepo<Style>().Delete(style);
            await _unitOfWork.CompleteAsync();
        }


        public async Task<IEnumerable<StyleResponse>> GetAllStyle()
        {
            var list = await _unitOfWork.GetRepo<Style>().GetAllAsync();
            return _mapper.Map<IEnumerable<StyleResponse>>(list); 
        }
        public async Task<StyleResponse> GetStyleById(Guid id)
        {
            var style = await _unitOfWork.GetRepo<Style>().GetByIdAsync(id);
            return _mapper.Map<StyleResponse>(style);

        }

        public async Task UpdateStyle(Guid id, StyleRequest styleRequest)
        {
            var style = await _unitOfWork.GetRepo<Style>().GetByIdAsync(id);
            if (style == null)
            {
                throw new KeyNotFoundException($"Style with ID {id} not found.");
            }

            style.Name = !string.IsNullOrEmpty(styleRequest.Name) ? styleRequest.Name : style.Name;
            style.Description = !string.IsNullOrEmpty(styleRequest.Description) ? styleRequest.Description : style.Description;
            style.Note = !string.IsNullOrEmpty(styleRequest.Note) ? styleRequest.Note : style.Note;
            style.Status = styleRequest.Status ?? style.Status;
            style.UpdateAt = DateTime.Now;
            _unitOfWork.GetRepo<Style>().Update(style);
            await _unitOfWork.CompleteAsync();
        }
    }
}
