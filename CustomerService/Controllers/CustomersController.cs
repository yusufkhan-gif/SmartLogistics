using CustomerService.Application.DTOs;
using CustomerService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CustomerService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }


        // Customer creates their own profile
        [HttpPost]
        public async Task<IActionResult> Create(CreateCustomerRequest request)
        {
            var userId = GetUserId();

            try
            {
                var result =
                    await _customerService.CreateAsync(
                        userId,
                        request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }


        // Customer gets own profile
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetUserId();

            var result =
                await _customerService.GetByUserIdAsync(
                    userId);

            if (result == null)
                return NotFound();

            return Ok(result);
        }


        // Customer updates own profile
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            UpdateCustomerRequest request)
        {
            var userId = GetUserId();

            var result =
                await _customerService.UpdateMyProfileAsync(
                    userId,
                    request);

            if (!result)
                return NotFound();

            return NoContent();
        }


        // Admin gets all customers
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result =
                await _customerService.GetAllAsync();

            return Ok(result);
        }


        // Admin gets customer by ID
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id)
        {
            var result =
                await _customerService.GetByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }


        // Admin updates customer
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            UpdateCustomerRequest request)
        {
            var result =
                await _customerService.UpdateAsync(
                    id,
                    request);

            if (!result)
                return NotFound();

            return NoContent();
        }


        // Admin deletes customer
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            Guid id)
        {
            var result =
                await _customerService.DeleteAsync(id);

            if (!result)
                return NotFound();

            return NoContent();
        }


        private Guid GetUserId()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)
                ??
                User.FindFirstValue(
                    JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(
                    userId,
                    out var userIdGuid))
            {
                throw new UnauthorizedAccessException(
                    "Invalid user ID in access token.");
            }

            return userIdGuid;
        }
    }
}
