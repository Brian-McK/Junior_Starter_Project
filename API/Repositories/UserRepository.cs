using API.Interfaces;
using API.Models;
using MongoDB.Driver;

namespace API.Repositories;

public class UserRepository: IUserRepository
{
    private readonly IMongoDbContext _mongoDbContext;
    
    public UserRepository(IMongoDbContext mongoDbContext)
    {
        _mongoDbContext = mongoDbContext;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _mongoDbContext.Users.Find(_ => true).ToListAsync();
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        return await _mongoDbContext.Users.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddAsync(User user)
    {
        await _mongoDbContext.Users.InsertOneAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        await _mongoDbContext.Users.ReplaceOneAsync(p => p.Id == user.Id, user);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _mongoDbContext.Users.DeleteOneAsync(p => p.Id == id);
    }
}