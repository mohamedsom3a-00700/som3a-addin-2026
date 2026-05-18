using Som3a.Shared.Models;
using System.Collections.Generic;

namespace Som3a.Shared.Core
{
    public class GraphBuilder
    {
        public Dictionary<string, List<Relationship>> Graph { get; private set; } = new();

        public void Build(List<Activity> acts, List<Relationship> rels)
        {
            foreach (var a in acts)
                Graph[a.Id] = new List<Relationship>();

            foreach (var r in rels)
                Graph[r.PredecessorId].Add(r);
        }
    }
}