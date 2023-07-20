using API.Data;
using API.Interfaces;
using API.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using BsonObjectId = Newtonsoft.Json.Bson.BsonObjectId;

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

    public async Task<SkillLevel> GetByIdAsync(string id)
    {
        return await _mongoDbContext.SkillLevels.Find(p => p.Id.Equals(id)).FirstOrDefaultAsync();
    }
    
    public async Task<SkillLevel> GetByNameAsync(string name)
    {
        var filter = Builders<SkillLevel>.Filter.Eq(u => u.Name, name);
        
        return await _mongoDbContext.SkillLevels.Find(filter).FirstOrDefaultAsync();
    }

    public async Task AddAsync(SkillLevel skillLevel)
    {
        await _mongoDbContext.SkillLevels.InsertOneAsync(skillLevel);
    }

    // TODO - Re-write to use a transaction instead
    public async Task<bool> UpdateAsync(SkillLevel skillLevel)
    {
      var updateSkillLevelResult = await _mongoDbContext.SkillLevels.ReplaceOneAsync(p => p.Id == skillLevel.Id, skillLevel);
      
      if (updateSkillLevelResult.ModifiedCount > 0)
      {
          return await UpdatedEmployeesWithSkillLevelMutations(skillLevel, "update");
      }

      return false;
    }

    // TODO - Re-write to use a transaction instead
    public async Task<bool> DeleteAsync(string id)
    {
        var skillLevel = await _mongoDbContext.SkillLevels.Find(s => s.Id.Equals(id)).FirstOrDefaultAsync();
    
        if (skillLevel == null)
        {
            return false;
        }
        
        var deleteResult = await _mongoDbContext.SkillLevels.DeleteOneAsync(s => s.Id.Equals(skillLevel.Id));
    
        if (deleteResult.DeletedCount > 0)
        {
            return await UpdatedEmployeesWithSkillLevelMutations(skillLevel, "delete");
        }
        
        return false;
    }
    
    private async Task<bool> UpdatedEmployeesWithSkillLevelMutations(SkillLevel skillLevelMutated, string mutationType)
    {
        if (mutationType == "delete")
        {
            var skillLevelId = ObjectId.Parse(skillLevelMutated.Id);

            var employeeSkillIdsFilter = Builders<Employee>.Filter.AnyEq("SkillLevelIds", skillLevelId);
            var updateSkillIds = Builders<Employee>.Update.Pull("SkillLevelIds", skillLevelId);
            var updatedEmployeesSkillLevelIds = await _mongoDbContext.Employees.UpdateManyAsync(employeeSkillIdsFilter, updateSkillIds);

            if (updatedEmployeesSkillLevelIds.ModifiedCount <= 0)
                return false;
        }

        var employeeSkillsFilter = Builders<Employee>.Filter.ElemMatch(e => e.SkillLevels, s => s.Id == skillLevelMutated.Id);
        var updateSkillLevels = Builders<Employee>.Update.Set("SkillLevels", skillLevelMutated);
        var updatedEmployeesSkillLevels =  await _mongoDbContext.Employees.UpdateManyAsync(employeeSkillsFilter, updateSkillLevels);

        return updatedEmployeesSkillLevels.ModifiedCount > 0;
    }
}