using API.Models;
using MongoDB.Driver;

namespace API.Interfaces;

public interface IMongoDbContext
{
    IMongoCollection<User> Users { get; }
    IMongoCollection<Employee> Employees { get; }
    IMongoCollection<SkillLevel> SkillLevels { get; }
}