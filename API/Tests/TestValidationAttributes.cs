using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using API.DTO;
using API.Tests.MockData;
using Xunit;

namespace API.Tests;

public class TestValidationAttributes
{
    #region AddAndUpdateEmployee

    [Fact]
    public void AddEmployeeRequest_Attributes_Valid()
    {
        var addEmployeeUserReq = EmployeesMockData.GetAddEmployeeRequest();

        var context = new ValidationContext(addEmployeeUserReq);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(addEmployeeUserReq, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("2022-01-01")]
    public void AddEmployeeRequest_Attributes_DobGreaterThan18YearsOld_InValid(DateTime dob)
    {
        var addEmployeeUserReq = EmployeesMockData.GetAddEmployeeRequest();

        addEmployeeUserReq.Dob = dob;

        var context = new ValidationContext(addEmployeeUserReq);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(addEmployeeUserReq, context, results, true);

        Assert.False(isValid);
        Assert.NotEmpty(results);
    }

    [Theory]
    [MemberData(nameof(InvalidSkillIdListData))]
    public void AddEmployeeRequest_Attributes_InvalidFormattedSkillIds_InValid(List<string> skillLevelIds)
    {
        var addEmployeeUserReq = EmployeesMockData.GetAddEmployeeRequest();

        addEmployeeUserReq.SkillLevelIds = skillLevelIds;

        var context = new ValidationContext(addEmployeeUserReq);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(addEmployeeUserReq, context, results, true);

        Assert.False(isValid);
        Assert.NotEmpty(results);
    }

    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHI")]
    public void AddEmployeeRequest_Attributes_FirstNameEqualTo35Chars_Valid(string firstName)
    {
        var addEmployeeUserReq = EmployeesMockData.GetAddEmployeeRequest();

        addEmployeeUserReq.FirstName = firstName;

        var context = new ValidationContext(addEmployeeUserReq);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(addEmployeeUserReq, context, results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")]
    public void AddEmployeeRequest_Attributes_FirstNameGreaterThan35Chars_InValid(string firstName)
    {
        var addEmployeeUserReq = EmployeesMockData.GetAddEmployeeRequest();

        addEmployeeUserReq.FirstName = firstName;

        var context = new ValidationContext(addEmployeeUserReq);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(addEmployeeUserReq, context, results, true);

        Assert.False(isValid);
        Assert.NotEmpty(results);
    }

    public static IEnumerable<object[]> InvalidSkillIdListData()
    {
        yield return new object[] { new List<string> { "SkillId1", "SkillId2", "SkillId3" } };
    }

    #endregion

    #region UserLogin

    [Fact]
    public void Username_Required()
    {
        var userReqDto = new UserReqDto
        {
            Username = null,
            Password = "TestPassword123!"
        };

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(userReqDto);
        var isValid = Validator.TryValidateObject(userReqDto, context, validationResults, true);
        
        Assert.False(isValid);
        Assert.Equal("Username is required", validationResults[0].ErrorMessage);
    }
    
    [Fact]
    public void Password_Required()
    {
        var userReqDto = new UserReqDto
        {
            Username = "TestUsername123!",
            Password = null
        };

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(userReqDto);
        var isValid = Validator.TryValidateObject(userReqDto, context, validationResults, true);
        
        Assert.False(isValid);
        Assert.Equal("Password is required", validationResults[0].ErrorMessage);
    }
    
    [Theory]
    [InlineData("abcde")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPqrstuvwxyzABCDEFG")]
    public void Username_StringLengthValidation(string username)
    {
        var userReqDto = new UserReqDto
        {
            Username = username,
            Password = "TestPassword123!"
        };
        
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(userReqDto);
        var isValid = Validator.TryValidateObject(userReqDto, context, validationResults, true);
        
        Assert.False(isValid);
        Assert.Single(validationResults);
        Assert.Equal("Username must be between 6 and 50 characters", validationResults[0].ErrorMessage);
    }
    
    [Theory]
    [InlineData("ValidPassword!?")]
    [InlineData("invalidpassword")]
    public void Password_RegexValidation(string password)
    {
        var userReqDto = new UserReqDto
        {
            Username = "TestUsername",
            Password = password
        };
        
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(userReqDto);
        var isValid = Validator.TryValidateObject(userReqDto, context, validationResults, true);
        
        var isPasswordValid = Regex.IsMatch(password, @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@#\$%\^&\+=!])(?!.*\s).{8,}$");
        
        Assert.Equal(isPasswordValid, isValid);
    }

    #endregion
    
}