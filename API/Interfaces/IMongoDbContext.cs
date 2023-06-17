using API.Models;
using MongoDB.Driver;

namespace API.Interfaces;

public interface IMongoDbContext
{
    public IMongoCollection<User> Users { get; }
    public IMongoCollection<Employee> Employees { get; }
    public IMongoCollection<SkillLevel> SkillLevels { get; }
}