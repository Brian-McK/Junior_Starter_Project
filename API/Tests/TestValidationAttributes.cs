using System.ComponentModel.DataAnnotations;
using API.Tests.MockData;
using Xunit;

namespace API.Tests;

public class TestValidationAttributes
{
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
    [InlineData("2022-01-01" )]
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
}