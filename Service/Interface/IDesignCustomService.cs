using BusinessObject.DTO.Delivery;
using BusinessObject.DTO.DesignCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IDesignCustomService
    {
        Task CreateDesignCustomByCustomer(DesignCustomByCustomerRequest designCustomByCustomerRequest, Guid customerId);
        Task UpdateDesignCustomByStaff(DesignCustomByStaffRequest designCustomByStaffRequest, Guid DesginCustom);
        Task<Guid> UpdateDesignCustomByCustomer(UpdateOrderDesignCustomByCustomer updateOrderDesignCustomByCustomer, Guid DesginCustom);

        Task<IEnumerable<DesignCustomBuCustomerResponse>> GetDesignCustomByCustomer(Guid customer);
        Task<IEnumerable<DesignCustomBuCustomerResponse>> GetDesignCustomByStore(Guid store);
        Task<IEnumerable<DesignCustomBuCustomerResponse>> GetDesignCustomByStaff(Guid staff);


        Task<DesignCustomBuCustomerResponse> GetDesignCustomById(Guid id);
        Task DeleteDesignCustom(Guid id);

    }
}
