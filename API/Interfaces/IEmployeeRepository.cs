using API.Models;

namespace API.Interfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    
    Task<Employee> GetByIdAsync(Guid id);
    
    Task AddAsync(Employee employee);
    
    Task UpdateAsync(Employee employee);
    
    Task DeleteAsync(Guid id);
}