using System.Globalization;
using System.Windows.Controls;

namespace TK158.Helpers
{
    class AddressInputValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return value.ToString().Length < 4
                ? new ValidationResult(false, "Введите 4 разряда.")
                : ValidationResult.ValidResult;
        }
    }
}
