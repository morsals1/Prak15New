using System.Globalization;
using System.Windows.Controls;

namespace Prak15Mensh.ValidationRules
{
    public class CategoryValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(false, "Название категории не может быть пустым");
            }

            string name = value.ToString();

            if (name.Length > 100)
                return new ValidationResult(false, "Название категории не может превышать 100 символов");

            return ValidationResult.ValidResult;
        }
    }
}