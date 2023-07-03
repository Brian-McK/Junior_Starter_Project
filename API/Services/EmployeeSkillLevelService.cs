using API.DTO;
using API.Interfaces;
using API.Models;
using API.Repositories;

namespace API.Services;

// Service layer for managing the business logic

public class EmployeeSkillLevelService: IEmployeeSkillLevelService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ISkillLevelRepository _skillLevelRepository;
    
    public EmployeeSkillLevelService(IUserRepository userRepository, IEmployeeRepository employeeRepository, ISkillLevelRepository skillLevelRepository)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _skillLevelRepository = skillLevelRepository;
    }

    #region User

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();

        return users;
    }

    public async Task<User> GetUserByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        return user;
    }
    
    public async Task<User> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetUserByUsernameAsync(username);

        return user;
    }

    public async Task AddUserAsync(User user)
    {
        await _userRepository.AddAsync(user);
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        var isUpdatedUser =  await _userRepository.UpdateAsync(user);

        return isUpdatedUser;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var isDeletedUser = await _userRepository.DeleteAsync(id);

        return isDeletedUser;
    }
    
    #endregion

    #region Employee

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        var employees = await _employeeRepository.GetAllAsync();

        return employees;
    }

    public async Task<Employee> GetEmployeeByIdAsync(string id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);

        return employee;
    }
    
    public async Task<Employee> GetEmployeeByEmailAsync(string email)
    {
        var employee = await _employeeRepository.GetByEmailAsync(email);

        return employee;
    }

    public async Task AddEmployeeAsync(Employee employee)
    {
        await _employeeRepository.AddAsync(employee);
    }

    public async Task<bool> UpdateEmployeeAsync(Employee employee)
    {
        var isUpdatedEmployee = await _employeeRepository.UpdateAsync(employee);

        return isUpdatedEmployee;
    }

    public async Task<bool> DeleteEmployeeAsync(string id)
    {
        var isDeletedEmployee = await _employeeRepository.DeleteAsync(id);

        return isDeletedEmployee;
    }

    #endregion

    #region SkillLevel

    public async Task<List<SkillLevel>> GetAllSkillLevelsAsync()
    {
        var skillLevels = await _skillLevelRepository.GetAllAsync();

        return skillLevels;
    }

    public async Task<SkillLevel> GetSkillLevelByIdAsync(string id)
    {
        var skillLevel = await _skillLevelRepository.GetByIdAsync(id);

        return skillLevel;
    }
    
    public async Task<SkillLevel> GetSkillLevelByNameAsync(string name)
    {
        var skillLevel = await _skillLevelRepository.GetByNameAsync(name);

        return skillLevel;
    }

    public async Task AddSkillLevelAsync(SkillLevel skillLevel)
    {
        await _skillLevelRepository.AddAsync(skillLevel);
    }

    public async Task<bool> UpdateSkillLevelAsync(SkillLevel skillLevel)
    {
        var isUpdatedSkillLevel = await _skillLevelRepository.UpdateAsync(skillLevel);

        return isUpdatedSkillLevel;
    }

    public async Task<bool> DeleteSkillLevelAsync(string id)
    {
        var isDeletedSkillLevel = await _skillLevelRepository.DeleteAsync(id);

        return isDeletedSkillLevel;
    }

    #endregion
    
}