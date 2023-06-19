using API.Models;

namespace API.Interfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    
    Task<Employee> GetByIdAsync(string id);
    
    Task AddAsync(Employee employee);
    
    Task<bool> UpdateAsync(Employee employee);
    
    Task<bool> DeleteAsync(Guid id);
}