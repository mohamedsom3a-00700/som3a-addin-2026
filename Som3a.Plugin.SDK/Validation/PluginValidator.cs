using System;
using System.Collections.Generic;
using System.Linq;
using Som3a.Contracts;
using Som3a.Plugin.SDK.Discovery;

namespace Som3a.Plugin.SDK.Validation
{
    public class PluginValidator
    {
        private readonly ContractVerifier _contractVerifier;

        public PluginValidator(ContractVerifier contractVerifier)
        {
            _contractVerifier = contractVerifier;
        }

        public List<PluginDescriptor> Validate(List<PluginDescriptor> descriptors)
        {
            var validDescriptors = new List<PluginDescriptor>();

            foreach (var descriptor in descriptors)
            {
                if (descriptor.PluginType == null)
                {
                    descriptor.Status = PluginStatus.Error;
                    continue;
                }

                var contractResult = _contractVerifier.Verify(descriptor.PluginType);
                if (!contractResult.IsSuccess)
                {
                    descriptor.Status = PluginStatus.Error;
                    continue;
                }

                var depsSatisfied = DependenciesSatisfied(descriptor, descriptors);
                if (!depsSatisfied)
                {
                    descriptor.Status = PluginStatus.Error;
                    continue;
                }

                descriptor.Status = PluginStatus.Loaded;
                validDescriptors.Add(descriptor);
            }

            ResolveConflicts(validDescriptors);

            return validDescriptors;
        }

        private bool DependenciesSatisfied(PluginDescriptor descriptor, List<PluginDescriptor> allDescriptors)
        {
            foreach (var depId in descriptor.Dependencies)
            {
                if (!allDescriptors.Any(d => d.Id == depId && d.Status == PluginStatus.Loaded))
                    return false;
            }
            return true;
        }

        private void ResolveConflicts(List<PluginDescriptor> descriptors)
        {
            var seenIds = new HashSet<string>();
            for (int i = descriptors.Count - 1; i >= 0; i--)
            {
                if (!seenIds.Add(descriptors[i].Id))
                {
                    descriptors[i].Status = PluginStatus.Disabled;
                }
            }
        }
    }
}
