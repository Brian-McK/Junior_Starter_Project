using System.Globalization;
using API.DTO;
using API.Models;
using API.Tests.MockData;
using MongoDB.Bson;

namespace API.Tests.MockData;

public class EmployeesMockData
{
    public static List<Employee> GetEmployees()
    {
        return new List<Employee>
        {
            new Employee
            {
                Id = "64a683e2fc13ae0f1ebeed1e",
                FirstName = "Lyman",
                LastName = "Sweetland",
                Dob = DateTime.ParseExact("26/10/1991", "dd/MM/yyyy", null),
                Email = "lsweetland0@mashable.com",
                IsActive = true,
                SkillLevelIds = SkillLevelsMockData.GetSkillLevels().Select(item => item.Id).ToList(),
            },
            new Employee
            {
                Id = "64a683e2fc13ae0f1ebeed29",
                FirstName = "Johna",
                LastName = "Round",
                Dob = DateTime.ParseExact("13/06/1992", "dd/MM/yyyy", null),
                Email = "cloomis4@naver.com",
                IsActive = false,
                SkillLevelIds = SkillLevelsMockData.GetSkillLevels().Select(item => item.Id).ToList().GetRange(1,2),
            },
            new Employee
            {
                Id = "64a683e2fc13ae0f1ebeed2c",
                FirstName = "Allistir",
                LastName = "Cleve",
                Dob = DateTime.ParseExact("13/09/1989", "dd/MM/yyyy", null),
                Email = "crolline@istockphoto.com",
                IsActive = true,
                SkillLevelIds = SkillLevelsMockData.GetSkillLevels().Select(item => item.Id).ToList().GetRange(0,1),
            },
        };
    }

    public static AddEmployeeRequestDto GetAddEmployeeRequest()
    {
        return new AddEmployeeRequestDto
        {
            FirstName = "Mary",
            LastName = "McEntee",
            Dob = DateTime.UtcNow.AddYears(-19),
            Email = "mary@hello.com",
            SkillLevelIds = SkillLevelsMockData.GetSkillLevels().Select(item => new ObjectId(item.Id).ToString()).ToList(),
            IsActive = true
        };
    }
}