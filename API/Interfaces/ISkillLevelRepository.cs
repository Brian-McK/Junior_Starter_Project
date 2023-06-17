using API.Models;

namespace API.Interfaces;

public interface ISkillLevelRepository
{
    Task<List<SkillLevel>> GetAllAsync();
    
    Task<SkillLevel> GetByIdAsync(Guid id);
    
    Task AddAsync(SkillLevel skillLevel);
    
    Task<bool> UpdateAsync(SkillLevel skillLevel);
    
    Task<bool> DeleteAsync(Guid id);
}