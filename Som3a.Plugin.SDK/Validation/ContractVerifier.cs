using System;
using Som3a.Contracts;

namespace Som3a.Plugin.SDK.Validation
{
    public class ContractVerifier
    {
        public ValidationResult Verify(Type pluginType)
        {
            if (pluginType == null)
                return ValidationResult.Failure("Plugin type is null.");

            if (!typeof(IPlugin).IsAssignableFrom(pluginType))
                return ValidationResult.Failure($"Type '{pluginType.FullName}' does not implement IPlugin.");

            var ctor = pluginType.GetConstructor(Type.EmptyTypes);
            if (ctor == null)
                return ValidationResult.Failure($"Type '{pluginType.FullName}' must have a parameterless constructor.");

            return ValidationResult.Success();
        }
    }
}
