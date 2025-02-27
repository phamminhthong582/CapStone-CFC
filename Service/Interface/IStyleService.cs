using BusinessObject.DTO.Style;
using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IStyleService
    {
        Task<IEnumerable<StyleResponse>> GetAllStyle();
        Task CreateStyle(StyleRequest styleRequest);
        Task DeleteStyle(Guid id);
        Task UpdateStyle(Guid id, StyleRequest styleRequest);
        Task<StyleResponse> GetStyleById(Guid id); 
    }
}
