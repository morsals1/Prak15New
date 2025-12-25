using System.Globalization;
using System.Windows.Controls;

namespace Prak15Mensh.ValidationRules
{
    public class RequiredIdValidationRule : ValidationRule
    {
        public string FieldName { get; set; } = "Поле";

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || (value is int intValue && intValue == 0))
            {
                return new ValidationResult(false, $"Необходимо выбрать {FieldName}");
            }
            return ValidationResult.ValidResult;
        }
    }
}