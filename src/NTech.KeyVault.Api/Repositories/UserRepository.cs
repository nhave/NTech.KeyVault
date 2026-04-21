using Microsoft.EntityFrameworkCore;
using NTech.KeyVault.Api.Data;
using NTech.KeyVault.Common.Models.Database;

namespace NTech.KeyVault.Api.Repositories
{
    public interface IUserRepository
    {
        public Task AddUser(User user);
        public Task<User?> GetById(string id, Func<IQueryable<User>, IQueryable<User>>? include = null);
        public Task<User?> GetByUsername(string username, Func<IQueryable<User>, IQueryable<User>>? include = null);
        public Task<User?> GetByEmail(string lookupHash, Func<IQueryable<User>, IQueryable<User>>? include = null);
        public Task Update(User user);
        public Task Delete(User user);
    }

    public class UserRepository(AppDbContext dbContext) : IUserRepository
    {
        public async Task AddUser(User user)
        {
            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();
        }

        public async Task<User?> GetById(
            string id,
            Func<IQueryable<User>, IQueryable<User>>? include = null)
        {
            IQueryable<User> query = dbContext.Users;

            if (include != null)
                query = include(query);

            return await query.FirstOrDefaultAsync(u => u.Id == Guid.Parse(id));
        }

        public async Task<User?> GetByUsername(
            string username,
            Func<IQueryable<User>, IQueryable<User>>? include = null)
        {
            IQueryable<User> query = dbContext.Users;

            if (include != null)
                query = include(query);

            return await query.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmail(
            string lookupHash,
            Func<IQueryable<User>, IQueryable<User>>? include = null)
        {
            IQueryable<User> query = dbContext.Users;

            if (include != null)
                query = include(query);

            return await query.FirstOrDefaultAsync(u => u.EmailLookupHash == lookupHash);
        }

        public async Task Update(User user)
        {
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();
        }

        public async Task Delete(User user)
        {
            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
        }
    }
}
