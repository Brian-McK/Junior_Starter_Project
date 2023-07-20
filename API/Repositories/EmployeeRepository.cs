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
        var pipeline = CreateEmployeePipeline();
        
        var result = await pipeline.ToListAsync();
        
        return result;
    }

    public async Task<Employee> GetByIdAsync(string id)
    {
        var pipeline = CreateEmployeePipeline()
            .Match(Builders<Employee>.Filter.Eq(e => e.Id, id));

        var result = await pipeline.FirstOrDefaultAsync();
        
        return result;
    }
    
    public async Task<Employee> GetByEmailAsync(string email)
    {
        return await _mongoDbContext.Employees.Find(p => p.Email.Equals(email)).FirstOrDefaultAsync();
    }

    public async Task AddAsync(Employee employee)
    {
        var skillLevelPipeline = await GetSkillLevelPipeline(employee).ToListAsync();

        employee.SkillLevels = skillLevelPipeline; 

        await _mongoDbContext.Employees.InsertOneAsync(employee);
    }

    public async Task<bool> UpdateAsync(Employee employee)
    {
        var skillLevelPipeline = await GetSkillLevelPipeline(employee).ToListAsync();

        employee.SkillLevels = skillLevelPipeline; 

        var updateResult = await _mongoDbContext.Employees.ReplaceOneAsync(p => p.Id == employee.Id, employee);

      return updateResult.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
      var deleteResult = await _mongoDbContext.Employees.DeleteOneAsync(p => p.Id.Equals(id));

      return deleteResult.DeletedCount > 0;
    }
    
    private IAggregateFluent<SkillLevel> GetSkillLevelPipeline(Employee employee)
    {
        return _mongoDbContext.SkillLevels.Aggregate()
            .Match(sl => employee.SkillLevelIds.Contains((ObjectId)(BsonValue)sl.Id));
    }
    
    private IAggregateFluent<Employee> CreateEmployeePipeline()
    {
        return _mongoDbContext.Employees.Aggregate()
            .Lookup(
                foreignCollection: _mongoDbContext.SkillLevels,
                localField: e => e.SkillLevelIds,
                foreignField: sl => sl.Id, 
                @as: (Employee e) => e.SkillLevels 
            );
    }
}