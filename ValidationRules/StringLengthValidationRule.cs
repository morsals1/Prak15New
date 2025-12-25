using System.Globalization;
using System.Windows.Controls;

namespace Prak15Mensh.ValidationRules
{
    public class StringLengthValidationRule : ValidationRule
    {
        private int _maxLength = 100;
        private string _fieldName = "Поле";

        public int MaxLength
        {
            get => _maxLength;
            set => _maxLength = value;
        }

        public string FieldName
        {
            get => _fieldName;
            set => _fieldName = value;
        }

        public bool ValidatesOnTargetUpdated { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null && value.ToString().Length > MaxLength)
            {
                return new ValidationResult(false, $"{FieldName} не может превышать {MaxLength} символов");
            }
            return ValidationResult.ValidResult;
        }
    }
}