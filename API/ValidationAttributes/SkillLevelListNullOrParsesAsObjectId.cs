using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;

namespace API.ValidationAttributes;

public class SkillLevelListNullOrParsesAsObjectId: ValidationAttribute
{
    protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
    {
        if (value is null or List<string> { Count: 0 })
        {
            return ValidationResult.Success;
        }
        
        if (value is List<string> skillLevelIds)
        {
            if (skillLevelIds.Any(id => !ObjectId.TryParse(id, out _)))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
        
        return new ValidationResult(ErrorMessage);
    }
}