using API.DTO;
using API.Models;

namespace API.Interfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllAsync();
    
    Task<Employee> GetByIdAsync(string id);
    
    Task<Employee> GetByEmailAsync(string email);
    
    Task AddAsync(Employee employee);
    
    Task<bool> UpdateAsync(Employee employee);
    
    Task<bool> DeleteAsync(string id);
}