using System.ComponentModel.DataAnnotations;

namespace API.ValidationAttributes;

public class MinDobAttribute: ValidationAttribute
{
    private readonly int _minimumAge;

    public MinDobAttribute(int minimumAge)
    {
        _minimumAge = minimumAge;
    }

    protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
    {
        if (value is DateTime Dob)
        {
            var currentDate = DateTime.Today;
            var minimumDate = currentDate.AddYears(-_minimumAge);

            if (Dob.Date > minimumDate.Date)
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
}