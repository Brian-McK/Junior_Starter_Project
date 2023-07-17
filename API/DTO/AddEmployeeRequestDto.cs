using System.ComponentModel.DataAnnotations;
using API.Models;
using API.ValidationAttributes;
using MongoDB.Bson;

namespace API.DTO;

public class AddEmployeeRequestDto
{
    [Required(ErrorMessage = "You must enter the first name")]
    [StringLength(35, ErrorMessage = "First name should not exceed 35 characters")]
    [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "First name should only contain alphabetic characters")]
    public string FirstName { get; set; }
    
    [Required(ErrorMessage = "You must enter the last name")]
    [StringLength(35, ErrorMessage = "Last name should not exceed 35 characters")]
    [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Last name should only contain alphabetic characters")]
    public string LastName { get; set; }
    
    [Required(ErrorMessage = "Date of birth can't be empty")]
    [DataType(DataType.Date, ErrorMessage = "Invalid Date")]
    [MinDob(18, ErrorMessage = "Date must be at least 18 years ago")]
    public DateTime? Dob { get; set; }
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }
    
    [SkillLevelListNullOrParsesAsObjectId(ErrorMessage = "Invalid skill level format")]
    public List<string> SkillLevelIds { get; set; }
    
    [Required(ErrorMessage = "Active or inactive is required")]
    public bool IsActive { get; set; }
}