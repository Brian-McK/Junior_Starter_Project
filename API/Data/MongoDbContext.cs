using API.Interfaces;
using API.Models;
using MongoDB.Driver;

namespace API.Data;

public class MongoDbContext: IMongoDbContext
{
    public IMongoCollection<User> Users { get; }
    public IMongoCollection<Employee> Employees { get; }
    public IMongoCollection<SkillLevel> SkillLevels { get; }
    
    public MongoDbContext(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetValue<string>("EmployeeSkillsDatabaseSettings:ConnectionString"));

        var db = client.GetDatabase(configuration.GetValue<string>("EmployeeSkillsDatabaseSettings:ConnectionString"));

        Users = db.GetCollection<User>(
            configuration.GetValue<string>("EmployeeSkillsDatabaseSettings:UserCollectionName"));
        
        Employees = db.GetCollection<Employee>(
            configuration.GetValue<string>("EmployeeSkillsDatabaseSettings:EmployeeCollectionName"));
        
        SkillLevels = db.GetCollection<SkillLevel>(
            configuration.GetValue<string>("EmployeeSkillsDatabaseSettings:SkillLevelCollectionName"));
    }
}