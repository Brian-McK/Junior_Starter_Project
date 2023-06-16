using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models;

public class Employee
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public Guid Id { get; set; }
    
    [Required]
    public string? FirstName { get; set; }
    
    [Required]
    public string? LastName { get; set; }
    
    [Required]
    public DateOnly? Dob { get; set; }
    
    [Required]
    public string? Email { get; set; }
    
    public SkillLevel? SkillLevel { get; set; }
    
    public bool IsActive { get; set; }
    
    public int Age { get; set; }
}