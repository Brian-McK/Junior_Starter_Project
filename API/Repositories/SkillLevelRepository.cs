using API.Interfaces;
using API.Models;
using MongoDB.Driver;

namespace API.Repositories;

public class SkillLevelRepository: ISkillLevelRepository
{
    private readonly IMongoDbContext _mongoDbContext;
    
    public SkillLevelRepository(IMongoDbContext mongoDbContext)
    {
        _mongoDbContext = mongoDbContext;
    }

    public async Task<List<SkillLevel>> GetAllAsync()
    {
        return await _mongoDbContext.SkillLevels.Find(_ => true).ToListAsync();
    }

    public async Task<SkillLevel> GetByIdAsync(Guid id)
    {
        return await _mongoDbContext.SkillLevels.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddAsync(SkillLevel skillLevel)
    {
        await _mongoDbContext.SkillLevels.InsertOneAsync(skillLevel);
    }

    public async Task UpdateAsync(SkillLevel skillLevel)
    {
        await _mongoDbContext.SkillLevels.ReplaceOneAsync(p => p.Id == skillLevel.Id, skillLevel);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _mongoDbContext.SkillLevels.DeleteOneAsync(p => p.Id == id);
    }
}