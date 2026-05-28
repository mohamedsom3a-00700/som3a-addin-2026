using Som3a.Domain.Activities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Som3a.Domain.Relationships
{
    public class RelationshipNetwork
    {
        private readonly Dictionary<string, List<string>> _adjacencyList = new();
        private readonly Dictionary<string, List<string>> _reverseAdjacencyList = new();
        private readonly HashSet<string> _allNodes = new();

        public IReadOnlyList<Activity> Activities { get; }
        public IReadOnlyList<Relationship> Relationships { get; }
        public IReadOnlyDictionary<string, List<string>> AdjacencyList => _adjacencyList;
        public IReadOnlyDictionary<string, List<string>> ReverseAdjacencyList => _reverseAdjacencyList;
        public int NodeCount => _allNodes.Count;
        public int EdgeCount => _adjacencyList.Values.Sum(v => v.Count);

        public RelationshipNetwork()
        {
            Activities = Array.Empty<Activity>();
            Relationships = Array.Empty<Relationship>();
        }

        public RelationshipNetwork(
            IReadOnlyList<Activity> activities,
            IReadOnlyList<Relationship> relationships)
        {
            Activities = activities ?? throw new ArgumentNullException(nameof(activities));
            Relationships = relationships ?? throw new ArgumentNullException(nameof(relationships));
            Build();
        }

        public void Build()
        {
            _adjacencyList.Clear();
            _reverseAdjacencyList.Clear();
            _allNodes.Clear();

            foreach (var activity in Activities)
            {
                var id = activity.ActivityId;
                _allNodes.Add(id);
                if (!_adjacencyList.ContainsKey(id))
                    _adjacencyList[id] = new List<string>();
                if (!_reverseAdjacencyList.ContainsKey(id))
                    _reverseAdjacencyList[id] = new List<string>();
            }

            foreach (var rel in Relationships)
            {
                var predId = rel.Predecessor?.ActivityId ?? string.Empty;
                var succId = rel.Successor?.ActivityId ?? string.Empty;

                if (string.IsNullOrEmpty(predId) || string.IsNullOrEmpty(succId))
                    continue;

                if (!_adjacencyList.ContainsKey(predId))
                    _adjacencyList[predId] = new List<string>();
                if (!_reverseAdjacencyList.ContainsKey(predId))
                    _reverseAdjacencyList[predId] = new List<string>();
                if (!_adjacencyList.ContainsKey(succId))
                    _adjacencyList[succId] = new List<string>();
                if (!_reverseAdjacencyList.ContainsKey(succId))
                    _reverseAdjacencyList[succId] = new List<string>();

                if (!_adjacencyList[predId].Contains(succId))
                    _adjacencyList[predId].Add(succId);
                if (!_reverseAdjacencyList[succId].Contains(predId))
                    _reverseAdjacencyList[succId].Add(predId);

                _allNodes.Add(predId);
                _allNodes.Add(succId);
            }
        }

        public bool HasCycles()
        {
            var visited = new HashSet<string>();
            var recStack = new HashSet<string>();

            foreach (var node in _allNodes)
            {
                if (HasCycleDfs(node, visited, recStack))
                    return true;
            }

            return false;
        }

        public List<string>? GetTopologicalOrder()
        {
            var inDegree = new Dictionary<string, int>();
            foreach (var node in _allNodes)
                inDegree[node] = 0;

            foreach (var kvp in _adjacencyList)
            {
                foreach (var neighbor in kvp.Value)
                {
                    if (inDegree.ContainsKey(neighbor))
                        inDegree[neighbor]++;
                }
            }

            var queue = new Queue<string>();
            foreach (var kvp in inDegree)
            {
                if (kvp.Value == 0)
                    queue.Enqueue(kvp.Key);
            }

            var result = new List<string>();
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                result.Add(node);

                if (_adjacencyList.TryGetValue(node, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        inDegree[neighbor]--;
                        if (inDegree[neighbor] == 0)
                            queue.Enqueue(neighbor);
                    }
                }
            }

            if (result.Count != _allNodes.Count)
                return null;

            return result;
        }

        public List<List<string>> GetConnectedComponents()
        {
            var visited = new HashSet<string>();
            var components = new List<List<string>>();

            foreach (var node in _allNodes)
            {
                if (!visited.Contains(node))
                {
                    var component = new List<string>();
                    var stack = new Stack<string>();
                    stack.Push(node);

                    while (stack.Count > 0)
                    {
                        var current = stack.Pop();
                        if (!visited.Add(current))
                            continue;

                        component.Add(current);

                        if (_adjacencyList.TryGetValue(current, out var neighbors))
                        {
                            foreach (var n in neighbors)
                            {
                                if (!visited.Contains(n))
                                    stack.Push(n);
                            }
                        }

                        if (_reverseAdjacencyList.TryGetValue(current, out var reverseNeighbors))
                        {
                            foreach (var n in reverseNeighbors)
                            {
                                if (!visited.Contains(n))
                                    stack.Push(n);
                            }
                        }
                    }

                    components.Add(component);
                }
            }

            return components;
        }

        private bool HasCycleDfs(string node, HashSet<string> visited, HashSet<string> recStack)
        {
            if (recStack.Contains(node)) return true;
            if (visited.Contains(node)) return false;

            visited.Add(node);
            recStack.Add(node);

            if (_adjacencyList.TryGetValue(node, out var neighbors))
            {
                foreach (var neighbor in neighbors)
                {
                    if (HasCycleDfs(neighbor, visited, recStack))
                        return true;
                }
            }

            recStack.Remove(node);
            return false;
        }
    }
}
