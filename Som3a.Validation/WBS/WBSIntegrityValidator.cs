using Som3a.Contracts;
using Som3a.Domain.WBS;

namespace Som3a.Validation.WBS
{
    public class WBSIntegrityValidator
    {
        public ValidationResult Validate(WBSNode root)
        {
            if (root.Parent != null)
                return ValidationResult.Failure("Root WBSNode must have null Parent.");

            var visited = new HashSet<string>();
            if (!ValidateNode(root, visited, 0, out var errorMessage))
                return ValidationResult.Failure(errorMessage!);

            return ValidationResult.Success();
        }

        private static bool ValidateNode(WBSNode node, HashSet<string> visited, int expectedLevel,
            out string? errorMessage)
        {
            if (!visited.Add(node.Id))
            {
                errorMessage = $"Cycle detected at WBSNode '{node.Code}' (Id={node.Id}).";
                return false;
            }

            if (node.Level != expectedLevel)
            {
                errorMessage = $"WBSNode '{node.Code}' Level={node.Level} but expected Level={expectedLevel}.";
                return false;
            }

            foreach (var child in node.Children)
            {
                if (child.Parent?.Id != node.Id)
                {
                    errorMessage = $"WBSNode '{child.Code}' has incorrect Parent reference.";
                    return false;
                }
                if (!ValidateNode(child, visited, expectedLevel + 1, out errorMessage))
                    return false;
            }

            visited.Remove(node.Id);
            errorMessage = null;
            return true;
        }
    }
}
