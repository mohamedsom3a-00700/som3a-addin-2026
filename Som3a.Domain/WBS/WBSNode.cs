using Som3a.Domain.Activities;

namespace Som3a.Domain.WBS
{
    public class WBSNode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Level
        {
            get
            {
                var level = 0;
                var current = Parent;
                while (current != null)
                {
                    level++;
                    current = current.Parent;
                }
                return level;
            }
        }
        public WBSNode? Parent { get; set; }
        public List<WBSNode> Children { get; set; } = new();
        public string FullPath
        {
            get
            {
                var parts = new Stack<string>();
                var current = this;
                while (current != null)
                {
                    parts.Push(current.Name);
                    current = current.Parent;
                }
                return string.Join("/", parts);
            }
        }
        public List<Activity> Activities { get; set; } = new();

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Code))
                throw new InvalidOperationException("WBSNode Code must not be empty.");

            if (HasCycle(this, new HashSet<string>()))
                throw new InvalidOperationException("WBSNode tree contains a cycle.");

            var computedLevel = 0;
            var p = Parent;
            while (p != null) { computedLevel++; p = p.Parent; }
            if (computedLevel != Level)
                throw new InvalidOperationException(
                    $"WBSNode Level ({Level}) doesn't match actual depth ({computedLevel}).");
        }

        private static bool HasCycle(WBSNode node, HashSet<string> visited)
        {
            if (!visited.Add(node.Id)) return true;
            foreach (var child in node.Children)
            {
                if (HasCycle(child, visited)) return true;
            }
            visited.Remove(node.Id);
            return false;
        }

        public void AddChild(WBSNode child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));
            if (Children.Contains(child))
                throw new ArgumentException("Child is already a child of this node.", nameof(child));
            if (child.Parent != null && child.Parent != this)
                throw new InvalidOperationException("Cannot reparent a WBSNode from another parent.");

            child.Parent = this;
            Children.Add(child);
        }
    }
}
