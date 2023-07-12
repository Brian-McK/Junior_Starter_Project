using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models;

public class RefreshTokenStore
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public string UserId { get; set; }
    
    public string RefreshToken { get; set; }
    
    public DateTime Created { get; set; }
    
    public bool IsValid { get; set; }
}