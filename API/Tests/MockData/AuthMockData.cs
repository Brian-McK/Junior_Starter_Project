using API.DTO;
using API.Models;

namespace API.Tests.MockData;

public class AuthMockData
{
    public static User GetUser()
    {
        return new User
        {
            Id = "64a68656fc13ae0e44beef3f",
            Username = "admintest",
            PasswordHash = GeneratePasswordHash("jimmy")
        };
    }
    
    public static UserReqDto GetUserWithReq()
    {
        return new UserReqDto
        {
            Username = "admintest",
            Password = "jimmy"
        };
    }
    
    public static User GetUserDoesntExist()
    {
        return new User
        {
            Id = "64a68656fc13ae0344beef1f",
            Username = "IDontExist",
            PasswordHash = GeneratePasswordHash("IDontExist")
        };
    }
    
    public static List<User> GetUsers()
    {
        return new List<User>
        {
            new()
            {
                Id = "64a68656fc13ae0e44beef7f",
                Username = "Brian",
                PasswordHash = GeneratePasswordHash("Brian")
            },
            new()
            {
                Id = "64a68656fc13ae0e44beef84",
                Username = "Jimmy",
                PasswordHash = GeneratePasswordHash("Jimmy")
            },
            new()
            {
                Id = "64a68656fc13ae0e44beef89",
                Username = "Mary",
                PasswordHash = GeneratePasswordHash("Mary")
            },
            new()
            {
                Id = "64a68656fc13ae0e44beef3f",
                Username = "admintest",
                PasswordHash = GeneratePasswordHash("jimmy")
            }
        };
    }
    
    private static string? GeneratePasswordHash(string password)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        
        return hashedPassword;
    }
}