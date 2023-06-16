using API.Interfaces;
using API.Models;
using MongoDB.Driver;

namespace API.Repositories;

public class SkillLevelRepository: ISkillLevelRepository
{
    private readonly IMongoCollection<SkillLevel> _skillLevelCollection;
    
    public SkillLevelRepository(IMongoDatabase database)
    {
        _skillLevelCollection = database.GetCollection<SkillLevel>("SkillLevels");
    }

    public async Task<List<SkillLevel>> GetAllAsync()
    {
        return await _skillLevelCollection.Find(_ => true).ToListAsync();
    }

    public async Task<SkillLevel> GetByIdAsync(Guid id)
    {
        return await _skillLevelCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddAsync(SkillLevel skillLevel)
    {
        await _skillLevelCollection.InsertOneAsync(skillLevel);
    }

    public async Task UpdateAsync(SkillLevel skillLevel)
    {
        await _skillLevelCollection.ReplaceOneAsync(p => p.Id == skillLevel.Id, skillLevel);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _skillLevelCollection.DeleteOneAsync(p => p.Id == id);
    }
}