using Som3a.Contracts;
using Som3a.Domain.BOQ;

namespace Som3a.Validation.BOQ
{
    public class BOQStructureValidator
    {
        public ValidationResult Validate(BOQDocument document)
        {
            var warnings = new List<string>();

            if (document.Sections.Count == 0)
                return ValidationResult.Failure("BOQDocument must have at least one section.");

            var itemCodes = new HashSet<string>();

            foreach (var section in document.Sections)
            {
                foreach (var item in section.Items)
                {
                    if (!itemCodes.Add(item.ItemCode))
                    {
                        return ValidationResult.Failure(
                            $"Duplicate BOQItem ItemCode '{item.ItemCode}' found in document.");
                    }

                    if (item.Quantity <= 0)
                    {
                        warnings.Add(
                            $"BOQItem '{item.ItemCode}' has non-positive quantity ({item.Quantity}).");
                    }
                }
            }

            if (warnings.Count > 0)
            {
                var result = ValidationResult.Success();
                result.Warnings = warnings;
                return result;
            }

            return ValidationResult.Success();
        }
    }
}
