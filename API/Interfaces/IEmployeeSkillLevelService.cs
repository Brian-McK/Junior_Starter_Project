using API.DTO;
using API.Models;

namespace API.Interfaces;

public interface IEmployeeSkillLevelService
{
    #region User

    Task<IEnumerable<User>> GetAllUsersAsync();
    
    Task<User> GetUserByIdAsync(string id);

    Task<User> GetUserByUsernameAsync(string username);
    
    Task AddUserAsync(User user);
    
    Task<bool> UpdateUserAsync(User user);
    
    Task<bool> DeleteUserAsync(string id);

    #endregion

    #region Employee

    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    
    Task<Employee> GetEmployeeByIdAsync(string id);
    
    Task AddEmployeeAsync(Employee employee);
    
    Task<bool> UpdateEmployeeAsync(Employee employee);
    
    Task<bool> DeleteEmployeeAsync(string id);

    #endregion
    
    #region SkillLevel

    Task<List<SkillLevel>> GetAllSkillLevelsAsync();
    
    Task<SkillLevel> GetSkillLevelByIdAsync(string id);
    
    Task AddSkillLevelAsync(SkillLevel skillLevel);
    
    Task<bool> UpdateSkillLevelAsync(SkillLevel skillLevel);
    
    Task<bool> DeleteSkillLevelAsync(string id);
   
    #endregion
}