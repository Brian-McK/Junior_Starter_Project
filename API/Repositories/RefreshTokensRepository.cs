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
    
    public async Task<bool> RefreshTokenExists(string id)
    {
        var filter = new FilterDefinitionBuilder<RefreshTokenStore>().Eq(r => r.Id, id);

        var count = await _mongoDbContext.RefreshTokens.CountDocumentsAsync(filter);

        return count > 0;
    }
}