using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models;

public class SkillLevel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public Guid Id { get; set; }
    
    [Required]
    public string? Name { get; set; }
    
    public string? Description { get; set; }
}