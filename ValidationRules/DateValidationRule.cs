using System;
using System.Globalization;
using System.Windows.Controls;

namespace Prak15Mensh.ValidationRules
{
    public class DateValidationRule : ValidationRule
    {
        public bool ValidatesOnTargetUpdated { get; set; }

        public bool AllowFutureDates { get; set; } = false;
        public bool AllowNull { get; set; } = false;
        public DateTime MinDate { get; set; } = new DateTime(2000, 1, 1);

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return AllowNull
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, "Дата не может быть пустой");
            }
            DateTime dateTime;
            if (value is string stringValue)
            {
                if (!DateTime.TryParse(stringValue, cultureInfo, DateTimeStyles.None, out dateTime))
                {
                    return new ValidationResult(false, "Некорректный формат даты");
                }
            }
            else if (value is DateTime dt)
            {
                dateTime = dt;
            }
            else
            {
                return new ValidationResult(false, "Некорректное значение даты");
            }
            if (!AllowFutureDates && dateTime > DateTime.Now)
            {
                return new ValidationResult(false, "Дата не может быть в будущем");
            }
            if (dateTime < MinDate)
            {
                return new ValidationResult(false, $"Дата не может быть раньше {MinDate:dd.MM.yyyy}");
            }

            return ValidationResult.ValidResult;
        }
    }
}