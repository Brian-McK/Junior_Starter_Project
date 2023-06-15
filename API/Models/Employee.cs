using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models;

public class Employee
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public string? FirstName { get; set; }
    
    [Required]
    public string? LastName { get; set; }
    
    [Required]
    public DateOnly? Dob { get; set; }
    
    [Required]
    public string? Email { get; set; }
    
    [ForeignKey("SkillLevelId")]
    public virtual SkillLevel? SkillLevel { get; set; }
    
    public bool IsActive { get; set; }
    
    public int Age { get; set; }
}