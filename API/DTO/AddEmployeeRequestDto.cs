using System.ComponentModel.DataAnnotations;
using API.Models;
using MongoDB.Bson;

namespace API.DTO;

public class AddEmployeeRequestDto
{
    [Required]
    public string? FirstName { get; set; }
    
    [Required]
    public string? LastName { get; set; }
    
    [Required]
    public DateTime? Dob { get; set; }
    
    [Required]
    public string? Email { get; set; }
    
    public List<string> SkillLevelIds { get; set; }
    
    public bool IsActive { get; set; }
}