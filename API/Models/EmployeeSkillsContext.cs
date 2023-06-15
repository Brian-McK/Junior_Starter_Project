using Microsoft.EntityFrameworkCore;

namespace API.Models;

public class EmployeeSkillsContext: DbContext
{
    public EmployeeSkillsContext(DbContextOptions<EmployeeSkillsContext> options) : base(options)
    {
    }
    
    public DbSet<Employee>? Employees { get; set; }
    
    public DbSet<User>? Users { get; set; }
    
    public DbSet<SkillLevel>? SkillLevels { get; set; }
}