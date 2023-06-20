using System.ComponentModel.DataAnnotations;
using API.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.DTO;

public class EmployeeEditDto
{
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }
    
    public DateTime? Dob { get; set; }
    
    public string? Email { get; set; }

    public bool IsActive { get; set; }
    
    public int Age { get; set; }
}