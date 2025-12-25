using System.Globalization;
using System.Windows.Controls;
using Prak15Mensh.Models;

namespace Prak15Mensh.ValidationRules
{
    public class BrandValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(false, "Название бренда не может быть пустым");
            }

            string name = value.ToString();

            if (name.Length > 100)
                return new ValidationResult(false, "Название бренда не может превышать 100 символов");

            return ValidationResult.ValidResult;
        }
    }
}