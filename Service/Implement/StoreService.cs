using BusinessObject.DTO.Request;
using BusinessObject.DTO.Response;
using BusinessObject.Entities;
using MailKit;
using Repository.Interface;
using Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement
{
    public class StoreService : IStoreService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StoreService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddStore(StoreRequest storeRequest)
        {
            var store = new Store
            {
                StoreName = storeRequest.StoreName,
                Address = storeRequest.Address, 
                StorePhone = storeRequest.StorePhone,
                StoreAvatar = storeRequest.StoreAvatar,
                StoreEmail = storeRequest.StoreEmail,
                BankAccountName = storeRequest.BankAccountName,
                BankName = storeRequest.BankName,
                BankNumber = storeRequest.BankNumber,
                MonoNumber = storeRequest.MonoNumber
            };
            await _unitOfWork.Repository<Store>().AddAsync(store);
            await _unitOfWork.CompleteAsync();

        }

        public Task DeleteStore(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<StoreResponse>> GetStore()
        {
            throw new NotImplementedException();
        }

        public Task<StoreResponse> GetStoreById(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateStore(StoreFlagsRequest storeFlagsRequest, Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
