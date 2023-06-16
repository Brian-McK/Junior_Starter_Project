using API.Interfaces;
using API.Models;
using MongoDB.Driver;

namespace API.Repositories;

public class UserRepository: IUserRepository
{
    private readonly IMongoCollection<User> _userCollection;
    
    public UserRepository(IMongoDatabase database)
    {
        _userCollection = database.GetCollection<User>("Users");
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _userCollection.Find(_ => true).ToListAsync();
    }

    public async Task<User> GetByIdAsync(Guid id)
    {
        return await _userCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddAsync(User user)
    {
        await _userCollection.InsertOneAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        await _userCollection.ReplaceOneAsync(p => p.Id == user.Id, user);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _userCollection.DeleteOneAsync(p => p.Id == id);
    }
}