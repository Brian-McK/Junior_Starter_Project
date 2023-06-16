namespace API.Models;

public class EmployeeSkillsDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string EmployeeCollectionName { get; set; } = null!;
    
    public string UserCollectionName { get; set; } = null!;
    
    public string SkillLevelCollectionName { get; set; } = null!;
}