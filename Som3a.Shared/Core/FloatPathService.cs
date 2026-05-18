using Som3a.Shared.Core;
using Som3a.Shared.Models;
using System.Collections.Generic;
using System.Linq;
public class FloatPathService
{
    public List<PathResult> GetTopPaths(
        string startId,
        Dictionary<string, List<Relationship>> graph,
        List<Activity> activities,
        int maxPaths = 20,
        int maxDepth = 100)
    {
        var results = new List<PathResult>();

        var finder = new PathFinder();

        var firstPath = finder.FindBestPath(
            startId,
            activities,
            graph,
            maxDepth,
            double.MaxValue
        );

        if (firstPath != null)
            results.Add(firstPath);

        // 🔥 optional: تقدر تزود variations بعد كدة
        return results;
    }
}