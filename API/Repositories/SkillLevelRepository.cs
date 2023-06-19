using API.Data;
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
        return await _mongoDbContext.SkillLevels.Find(p => p.Id.Equals(id)).FirstOrDefaultAsync();
    }

    public async Task AddAsync(SkillLevel skillLevel)
    {
        await _mongoDbContext.SkillLevels.InsertOneAsync(skillLevel);
    }

    public async Task<bool> UpdateAsync(SkillLevel skillLevel)
    {
      var updateSkillLevelResult = await _mongoDbContext.SkillLevels.ReplaceOneAsync(p => p.Id == skillLevel.Id, skillLevel);

      return updateSkillLevelResult.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
       var deleteSkillLevelResult = await _mongoDbContext.SkillLevels.DeleteOneAsync(p => p.Id.Equals(id));

       return deleteSkillLevelResult.DeletedCount > 0;
    }
}