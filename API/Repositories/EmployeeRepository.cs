using API.Interfaces;
using API.Models;
using MongoDB.Driver;

namespace API.Repositories;

public class EmployeeRepository: IEmployeeRepository
{
    private readonly IMongoCollection<Employee> _employeeCollection;

    public EmployeeRepository(IMongoDatabase database)
    {
        _employeeCollection = database.GetCollection<Employee>("Employees");
    }
    
    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _employeeCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Employee> GetByIdAsync(Guid id)
    {
        return await _employeeCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task AddAsync(Employee employee)
    {
        await _employeeCollection.InsertOneAsync(employee);
    }

    public async Task UpdateAsync(Employee employee)
    {
        await _employeeCollection.ReplaceOneAsync(p => p.Id == employee.Id, employee);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _employeeCollection.DeleteOneAsync(p => p.Id == id);
    }
}