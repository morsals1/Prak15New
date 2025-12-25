using System.Globalization;
using System.Windows.Controls;

namespace Prak15Mensh.ValidationRules
{
    public class RatingValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                string input = value.ToString();
                input = input.Replace(',', '.');

                if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double rating))
                {
                    if (rating < 0 || rating > 5)
                    {
                        return new ValidationResult(false, "Рейтинг должен быть от 0 до 5");
                    }

                    if (rating == 0)
                    {
                        return new ValidationResult(false, "Рейтинг должен быть больше 0");
                    }
                }
                else
                {
                    return new ValidationResult(false, "Введите число от 0 до 5 (например: 4.5)");
                }
            }
            else
            {
                return new ValidationResult(false, "Рейтинг не может быть пустым");
            }

            return ValidationResult.ValidResult;
        }
    }
}