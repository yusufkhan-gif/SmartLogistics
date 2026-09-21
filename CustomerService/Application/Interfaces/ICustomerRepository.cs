using CustomerService.Domain.Entities;

namespace CustomerService.Application.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(Guid id);

        Task<Customer?> GetByUserIdAsync(Guid userId);

        Task<IEnumerable<Customer>> GetAllAsync();

        Task<Customer> CreateAsync(Customer customer);

        Task UpdateAsync(Customer customer);

        Task DeleteAsync(Customer customer);
    }
}
