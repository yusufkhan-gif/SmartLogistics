using CustomerService.Application.DTOs;
using CustomerService.Application.Interfaces;
using CustomerService.Domain.Entities;

namespace CustomerService.Application.Services;

public class CustomerApplicationService : ICustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerApplicationService(
        ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerResponse> CreateAsync(
        Guid userId,
        CreateCustomerRequest request)
    {
        var existingCustomer =
            await _repository.GetByUserIdAsync(userId);

        if (existingCustomer != null)
        {
            throw new InvalidOperationException(
                "Customer profile already exists.");
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),

            UserId = userId,

            FirstName = request.FirstName.Trim(),

            LastName = request.LastName.Trim(),

            Email = request.Email
                .Trim()
                .ToLowerInvariant(),

            Phone = request.Phone?.Trim(),

            Address = request.Address?.Trim(),

            CreatedAt = DateTime.UtcNow,

            UpdatedAt = DateTime.UtcNow
        };

        var result =
            await _repository.CreateAsync(customer);

        return MapToResponse(result);
    }

    public async Task<CustomerResponse?> GetByIdAsync(
        Guid id)
    {
        var customer =
            await _repository.GetByIdAsync(id);

        return customer == null
            ? null
            : MapToResponse(customer);
    }

    public async Task<CustomerResponse?> GetByUserIdAsync(
        Guid userId)
    {
        var customer =
            await _repository.GetByUserIdAsync(userId);

        return customer == null
            ? null
            : MapToResponse(customer);
    }

    public async Task<IEnumerable<CustomerResponse>>
        GetAllAsync()
    {
        var customers =
            await _repository.GetAllAsync();

        return customers.Select(MapToResponse);
    }

    public async Task<bool> UpdateAsync(
        Guid id,
        UpdateCustomerRequest request)
    {
        var customer =
            await _repository.GetByIdAsync(id);

        if (customer == null)
            return false;

        customer.FirstName =
            request.FirstName.Trim();

        customer.LastName =
            request.LastName.Trim();

        customer.Phone =
            request.Phone?.Trim();

        customer.Address =
            request.Address?.Trim();

        customer.UpdatedAt =
            DateTime.UtcNow;

        await _repository.UpdateAsync(customer);

        return true;
    }

    public async Task<bool> UpdateMyProfileAsync(
        Guid userId,
        UpdateCustomerRequest request)
    {
        var customer =
            await _repository.GetByUserIdAsync(userId);

        if (customer == null)
            return false;

        customer.FirstName =
            request.FirstName.Trim();

        customer.LastName =
            request.LastName.Trim();

        customer.Phone =
            request.Phone?.Trim();

        customer.Address =
            request.Address?.Trim();

        customer.UpdatedAt =
            DateTime.UtcNow;

        await _repository.UpdateAsync(customer);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var customer =
            await _repository.GetByIdAsync(id);

        if (customer == null)
            return false;

        await _repository.DeleteAsync(customer);

        return true;
    }

    private static CustomerResponse MapToResponse(
        Customer customer)
    {
        return new CustomerResponse
        {
            Id = customer.Id,
            UserId = customer.UserId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}