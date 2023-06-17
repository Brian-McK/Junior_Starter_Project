using API.Interfaces;
using API.Models;
using MongoDB.Driver;

namespace API.Repositories;

public class EmployeeRepository: IEmployeeRepository
{
    private readonly IMongoDbContext _mongoDbContext;

    public EmployeeRepository(IMongoDbContext mongoDbContext)
    {
        _mongoDbContext = mongoDbContext;
    }
    
    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _mongoDbContext.Employees.Find(_ => true).ToListAsync();
    }

    public async Task<Employee> GetByIdAsync(Guid id)
    {
        return await _mongoDbContext.Employees.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddAsync(Employee employee)
    {
        await _mongoDbContext.Employees.InsertOneAsync(employee);
    }

    public async Task UpdateAsync(Employee employee)
    {
        await _mongoDbContext.Employees.ReplaceOneAsync(p => p.Id == employee.Id, employee);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _mongoDbContext.Employees.DeleteOneAsync(p => p.Id == id);
    }
}