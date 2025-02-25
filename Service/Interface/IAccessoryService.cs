using BusinessObject.DTO.Accessory;
using BusinessObject.DTO.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IAccessoryService
    {
        Task<IEnumerable<AccessoryResponse>> GetAllAccessory();
        Task CreateAccessory(AccessoryRequest accessoryRequest);
        Task DeleteAccessory(Guid id);
        Task UpdateAccessory(Guid id, AccessoryRequest accessoryRequest);
        Task<AccessoryResponse> GetAccessoryById(Guid id);
    }
}
