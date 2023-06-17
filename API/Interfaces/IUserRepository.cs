using API.Models;

namespace API.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    
    Task<User> GetByIdAsync(Guid id);
    
    Task AddAsync(User user);
    
    Task<bool> UpdateAsync(User user);
    
    Task<bool> DeleteAsync(Guid id);
}