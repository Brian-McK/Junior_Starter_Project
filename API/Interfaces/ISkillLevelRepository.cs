using API.Models;

namespace API.Interfaces;

public interface ISkillLevelRepository
{
    Task<List<SkillLevel>> GetAllAsync();
    
    Task<SkillLevel> GetByIdAsync(string id);

    Task<SkillLevel> GetByNameAsync(string name);
    
    Task AddAsync(SkillLevel skillLevel);
    
    Task<bool> UpdateAsync(SkillLevel skillLevel);
    
    Task<bool> DeleteAsync(string id);
}