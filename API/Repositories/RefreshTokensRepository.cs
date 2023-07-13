using API.Interfaces;
using API.Models;
using MongoDB.Driver;

namespace API.Repositories;

public class RefreshTokensRepository: IRefreshTokensRepository
{
    private readonly IMongoDbContext _mongoDbContext;
    
    public RefreshTokensRepository(IMongoDbContext mongoDbContext)
    {
        _mongoDbContext = mongoDbContext;
    }
    
    public async Task AddAsync(RefreshTokenStore refreshToken)
    {
        await _mongoDbContext.RefreshTokens.InsertOneAsync(refreshToken);
    }
    
    public async Task<bool> UpdateAsync(RefreshTokenStore refreshToken)
    {
        var updateResult = await _mongoDbContext.RefreshTokens.ReplaceOneAsync(p => p.Id == refreshToken.Id, refreshToken);

        return updateResult.ModifiedCount > 0;
    }
    
    public async Task DeleteAsync(string id)
    {
        var filter = Builders<RefreshTokenStore>.Filter.Eq("_id", id);

        await _mongoDbContext.RefreshTokens.DeleteOneAsync(filter);
    }

    public async Task<RefreshTokenStore?> GetRefreshToken(string userId, string refreshToken)
    {
        var filter = Builders<RefreshTokenStore>.Filter.Eq(t => t.UserId, userId) &
                     Builders<RefreshTokenStore>.Filter.Eq(t => t.RefreshToken, refreshToken);
        
        var result = await _mongoDbContext.RefreshTokens.Find(filter).FirstOrDefaultAsync();

        return result;
    }
}