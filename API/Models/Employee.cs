using System.ComponentModel.DataAnnotations;

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
    
    // add skill level ref here
    [Required]
    public bool IsActive { get; set; }

    [Required]
    public int Age { get; set; }
}