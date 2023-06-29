using API.Models;

namespace API.DTO;

public class EmployeeListSkillLevel
{
    public string? Id { get; set; }
    
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }
    
    public DateTime? Dob { get; set; }
    
    public string? Email { get; set; }

    public List<SkillLevel> SkillLevels { get; set; }

    public bool IsActive { get; set; } = false;

    public int Age { get; set; }
}