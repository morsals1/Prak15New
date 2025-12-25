using System.Globalization;
using System.Windows.Controls;

namespace Prak15Mensh.ValidationRules
{
    public class ProductNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(false, "Название товара не может быть пустым");
            }

            string name = value.ToString();

            if (name.Length > 200)
                return new ValidationResult(false, "Название товара не может превышать 200 символов");

            return ValidationResult.ValidResult;
        }
    }
}