using System.Globalization;
using System.Windows.Controls;

namespace Prak15Mensh.ValidationRules
{
    public class PriceFilterValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                if (!decimal.TryParse(value.ToString(), out decimal price))
                {
                    return new ValidationResult(false, "Цена должна быть числом");
                }

                if (price < 0)
                    return new ValidationResult(false, "Цена не может быть отрицательной");
            }
            return ValidationResult.ValidResult;
        }
    }
}