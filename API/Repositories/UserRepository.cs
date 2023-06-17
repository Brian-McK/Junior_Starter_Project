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

    public async Task<bool> UpdateAsync(User user)
    {
       var updateResult = await _mongoDbContext.Users.ReplaceOneAsync(p => p.Id == user.Id, user);

       return updateResult.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
      var deleteResult = await _mongoDbContext.Users.DeleteOneAsync(p => p.Id == id);

      return deleteResult.DeletedCount > 0;
    }
}