using API.DTO;
using API.Models;

namespace API.Interfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<EmployeeListSkillLevel>> GetAllAsync();
    
    Task<Employee> GetByIdAsync(string id);
    
    Task AddAsync(Employee employee);
    
    Task<bool> UpdateAsync(Employee employee);
    
    Task<bool> DeleteAsync(string id);
}