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
        public async Task<IEnumerable<StoreResponse>> GetStore()
        {
            var stores =  await _unitOfWork.Repository<Store>().GetAllAsync();
            var storeResponse = stores.Select(store => new StoreResponse
            {
                StoreId = store.StoreId,
                StoreName = store.StoreName,
                Address = store.Address,
                StorePhone = store.StorePhone,
                StoreAvatar = store.StoreAvatar,
                StoreEmail = store.StoreEmail,
                BankAccountName = store.BankAccountName,
                BankName = store.BankName,
                BankNumber = store.BankNumber,
                MonoNumber = store.MonoNumber,
                CreateAt = store.CreateAt,
                UpdateAt = store.UpdateAt,
                Status = store.Status
            });
            return storeResponse;
        }
        public async Task<StoreResponse> GetStoreById(Guid id)
        {
            var store = await _unitOfWork.Repository<Store>().GetByIdAsync(id);
            if (store == null)
            {
                throw new KeyNotFoundException("Store not found");
            }
            var storeResponse = new StoreResponse
            {
                StoreId = store.StoreId,
                StoreName = store.StoreName,
                Address = store.Address,
                StorePhone = store.StorePhone,
                StoreAvatar = store.StoreAvatar,
                StoreEmail = store.StoreEmail,
                BankAccountName = store.BankAccountName,
                BankName = store.BankName,
                BankNumber = store.BankNumber,
                MonoNumber = store.MonoNumber,
                CreateAt = store.CreateAt,
                UpdateAt = store.UpdateAt,
                Status = store.Status
            };

            return storeResponse;
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
                MonoNumber = storeRequest.MonoNumber, 
                CreateAt = DateTime.Now,
                Status = true
            };
            await _unitOfWork.Repository<Store>().AddAsync(store);
            await _unitOfWork.CompleteAsync();

        }

        public async Task DeleteStore(Guid id)
        {
            var store = await _unitOfWork.Repository<Store>().GetByIdAsync(id);
            if (store == null)
            {
                throw new KeyNotFoundException("Store not found");
            }
            _unitOfWork.Repository<Store>().Delete(store);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateStore(StoreRequest storeRequest, Guid id)
        {
            var store = await _unitOfWork.Repository<Store>().GetByIdAsync(id);
            if (store == null)
            {
                throw new KeyNotFoundException("Store not found");
            }
            store.StoreName = storeRequest.StoreName ?? store.StoreName;  // Giữ nguyên nếu giá trị mới là null
            store.Address = storeRequest.Address ?? store.Address;
            store.StorePhone = storeRequest.StorePhone ?? store.StorePhone;
            store.StoreAvatar = storeRequest.StoreAvatar ?? store.StoreAvatar;
            store.StoreEmail = storeRequest.StoreEmail ?? store.StoreEmail;
            store.BankAccountName = storeRequest.BankAccountName ?? store.BankAccountName;
            store.BankName = storeRequest.BankName ?? store.BankName;
            store.BankNumber = storeRequest.BankNumber ?? store.BankNumber;
            store.MonoNumber = storeRequest.MonoNumber ?? store.MonoNumber;
            store.Status = storeRequest.Status ?? store.Status;
            _unitOfWork.Repository<Store>().Update(store);
            await _unitOfWork.CompleteAsync();

        }
    }
}
