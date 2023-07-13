using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models;

public class RefreshTokenStore
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    [Required]
    public string RefreshToken { get; set; }
    
    [Required]
    public DateTime Created { get; set; }
}