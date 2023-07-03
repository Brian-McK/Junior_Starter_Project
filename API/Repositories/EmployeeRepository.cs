using API.Data;
using API.DTO;
using API.Interfaces;
using API.Models;
using MongoDB.Bson;
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
        var pipeline = _mongoDbContext.Employees.Aggregate()
            .Lookup(
                foreignCollection: _mongoDbContext.SkillLevels,
                localField: e => e.SkillLevelIds, // Property in the "employees" collection referencing skill levels
                foreignField: sl => sl.Id, // ID property in the "skillLevels" collection
                @as: (Employee e) => e.SkillLevels // Property in the "employees" collection to store the joined skill levels
            );
        
        var result = await pipeline.ToListAsync();

        return result;
    }

    public async Task<Employee> GetByIdAsync(string id)
    {
        return await _mongoDbContext.Employees.Find(p => p.Id.Equals(id)).FirstOrDefaultAsync();
    }
    
    public async Task<Employee> GetByEmailAsync(string email)
    {
        return await _mongoDbContext.Employees.Find(p => p.Email.Equals(email)).FirstOrDefaultAsync();
    }

    public async Task AddAsync(Employee employee)
    {
        await _mongoDbContext.Employees.InsertOneAsync(employee);
    }

    public async Task<bool> UpdateAsync(Employee employee)
    {
      var updateResult = await _mongoDbContext.Employees.ReplaceOneAsync(p => p.Id == employee.Id, employee);

      return updateResult.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
      var deleteResult = await _mongoDbContext.Employees.DeleteOneAsync(p => p.Id.Equals(id));

      return deleteResult.DeletedCount > 0;
    }
}