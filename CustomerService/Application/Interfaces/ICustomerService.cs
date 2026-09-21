using CustomerService.Application.DTOs;

namespace CustomerService.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<CustomerResponse> CreateAsync(
        Guid userId,
        CreateCustomerRequest request);

        Task<CustomerResponse?> GetByIdAsync(Guid id);

        Task<CustomerResponse?> GetByUserIdAsync(Guid userId);

        Task<IEnumerable<CustomerResponse>> GetAllAsync();

        Task<bool> UpdateAsync(
            Guid id,
            UpdateCustomerRequest request);

        Task<bool> UpdateMyProfileAsync(
            Guid userId,
            UpdateCustomerRequest request);

        Task<bool> DeleteAsync(Guid id);
    }
}
