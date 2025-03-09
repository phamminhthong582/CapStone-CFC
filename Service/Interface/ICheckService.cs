using BusinessObject.DTO.Check;
using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface ICheckService
    {
        Task<(bool isSuccess, double? distance)> CheckDeliveryByCheckOutProduct(Guid storeId, CheckProductRequest checkProductRequest);

        Task<(bool isSuccess, double? distance)> CheckDeliveryByProductCustom(Guid storeId, CheckProductFlowerRequest checkProductFlowerRequest);


    }
}
