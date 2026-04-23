using NTech.KeyVault.Api.Repositories;
using NTech.KeyVault.Common.Models.Dtos.Requests;

namespace NTech.KeyVault.Api.Services
{
    public interface IAdminService
    {
    }

    public class AdminService(IUserService userService) : IAdminService
    {
        public async Task CreateUser(CreateUserRequest dto)
        {
            await userService.CreateUserAsync(dto.Username, dto.FullName, dto.Email, dto.Password);
        }
    }
}
