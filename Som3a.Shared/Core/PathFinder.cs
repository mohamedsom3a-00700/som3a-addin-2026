using Som3a.Shared.Models;
using System.Collections.Generic;
using System.Linq;

namespace Som3a.Shared.Core
{
    public class PathFinder
    {
        private Dictionary<string, List<Relationship>> _graph;
        private Dictionary<string, Activity> _lookup;

        private int _maxDepth = 100;     // 🔥 أقصى طول للمسار
        private double _floatThreshold = double.MaxValue; // 🔥 فلتر بالـ Float

        public PathResult FindBestPath(
            string startId,
            List<Activity> activities,
            Dictionary<string, List<Relationship>> graph,
            int maxDepth = 100,
            double floatThreshold = double.MaxValue)
        {
            _graph = graph;
            _lookup = activities.ToDictionary(a => a.Id);

            _maxDepth = maxDepth;
            _floatThreshold = floatThreshold;

            return DFS(startId, new List<Activity>());
        }

        private PathResult DFS(string current, List<Activity> path)
        {
            // 🔁 منع loop
            if (path.Any(a => a.Id == current))
                return null;

            // 🔥 limit depth
            if (path.Count >= _maxDepth)
                return null;

            if (!_lookup.ContainsKey(current))
                return null;

            var act = _lookup[current];
            path.Add(act);

            // 🔥 فلتر بالـ float (اختياري)
            if (act.TotalFloat > _floatThreshold)
                return null;

            // نهاية المسار
            if (!_graph.ContainsKey(current) || _graph[current].Count == 0)
            {
                return new PathResult
                {
                    Activities = new List<Activity>(path),
                    PathFloat = path.Min(a => a.TotalFloat)
                };
            }

            PathResult best = null;

            // 🔥 ترتيب حسب float (Driving Logic)
            var nextList = _graph[current]
                .Where(r => _lookup.ContainsKey(r.SuccessorId))
                .OrderBy(r => _lookup[r.SuccessorId].TotalFloat)
                .ToList();

            foreach (var rel in nextList)
            {
                var result = DFS(rel.SuccessorId, new List<Activity>(path));

                if (result == null)
                    continue;

                if (best == null || result.PathFloat < best.PathFloat)
                    best = result;
            }

            return best;
        }
    }
}