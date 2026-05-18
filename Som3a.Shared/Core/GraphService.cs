using Som3a.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Som3a.Shared.Core
{
    public class GraphService
    {
        public string GenerateGraphHtml(
            List<Activity> acts,
            Dictionary<string, List<Relationship>> rels)
        {
            // ================= CLEAN DATA =================

            string Clean(string s)
            {
                return string.IsNullOrWhiteSpace(s)
                    ? Guid.NewGuid().ToString()
                    : s.Trim();
            }

            string Safe(string s)
            {
                return s?.Replace("'", "\\'") ?? "";
            }

            string FormatDate(DateTime? d)
            {
                return d.HasValue ? d.Value.ToString("dd MMM yyyy") : "";
            }

            // تنظيف activities
            acts = acts
                .Where(a => !string.IsNullOrWhiteSpace(a.Id))
                .GroupBy(a => a.Id.Trim())
                .Select(g => g.First())
                .ToList();

            var validIds = acts.Select(a => a.Id.Trim()).ToHashSet();

            // تنظيف العلاقات
            rels = rels
                .Where(k => validIds.Contains(k.Key?.Trim()))
                .ToDictionary(
                    k => k.Key.Trim(),
                    k => k.Value
                        .Where(r =>
                            validIds.Contains(r.PredecessorId?.Trim()) &&
                            validIds.Contains(r.SuccessorId?.Trim()))
                        .ToList()
                );

            // ================= NODES =================

            var nodes = string.Join(",", acts.Select(a => $@"
            {{
                data: {{
                    id: '{Clean(a.Id)}',
                    code: '{Safe(a.Code)}',
                    name: '{Safe(a.Name)}',
                    es: '{FormatDate(a.Start)}',
                    ef: '{FormatDate(a.Finish)}'
                }}
            }}"));

            // ================= EDGES =================

            var edges = string.Join(",", rels.SelectMany(r => r.Value)
                .Select(e => $@"
            {{
                data: {{
                    source: '{Clean(e.PredecessorId)}',
                    target: '{Clean(e.SuccessorId)}'
                }}
            }}"));

            // ================= HTML =================

            return $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>

<script src='https://unpkg.com/cytoscape@3.26.0/dist/cytoscape.min.js'></script>
<script src='https://unpkg.com/cytoscape-node-html-label@1.2.2/dist/cytoscape-node-html-label.js'></script>

<style>
html, body {{
    width:100%;
    height:100%;
    margin:0;
    background:#1e1e1e;
    font-family:Segoe UI;
}}

#cy {{
    width:100%;
    height:100%;
}}

.cy-node-html {{
    pointer-events:none;
}}

.expand-btn {{
    pointer-events:auto;
}}
</style>

</head>

<body>

<div id='cy'></div>

<script>

// ================= INIT =================

var cy = cytoscape({{
    container: document.getElementById('cy'),

    elements: {{
        nodes: [{nodes}],
        edges: [{edges}]
    }},

    style: [
        {{
            selector: 'node',
            style: {{
                'background-opacity': 0,
                'width': 220,
                'height': 110
            }}
        }},
        {{
            selector: 'edge',
            style: {{
                'width': 3,
                'line-color': '#00d4ff',
                'target-arrow-color': '#00d4ff',
                'target-arrow-shape': 'triangle',
                'curve-style': 'bezier'
            }}
        }}
    ],

    layout: {{
        name: 'cose',
        animate: true,
        fit: true,
        padding: 50
    }},

    userPanningEnabled: true,
    userZoomingEnabled: true
}});


// ================= DEBUG =================

alert('Nodes: ' + cy.nodes().length);
alert('Edges: ' + cy.edges().length);


// ================= SHOW ALL =================

cy.nodes().show();
cy.edges().show();


// ================= DRAG =================

cy.nodes().grabify();

cy.on('grab', 'node', e => e.target.style('opacity', 0.7));
cy.on('free', 'node', e => e.target.style('opacity', 1));


// ================= EXPAND =================

window.expandNode = function(id) {{

    var node = cy.getElementById(id);

    var nextNodes = node.outgoers('node');
    var nextEdges = node.outgoers('edge');

    var prevNodes = node.incomers('node');
    var prevEdges = node.incomers('edge');

    var expanded = node.data('expanded') === true;

    var btn = document.getElementById('btn_' + id);
    if (btn) {{
        btn.innerHTML = expanded ? '+' : '-';
    }}

    if (expanded) {{

        nextNodes.forEach(n => n.hide());
        nextEdges.forEach(e => e.hide());

        prevNodes.forEach(n => n.hide());
        prevEdges.forEach(e => e.hide());

        node.data('expanded', false);

    }} else {{

        nextNodes.forEach(n => n.show());
        nextEdges.forEach(e => e.show());

        prevNodes.forEach(n => n.show());
        prevEdges.forEach(e => e.show());

        node.data('expanded', true);
    }}

    cy.layout({{
        name: 'cose',
        animate: true
    }}).run();
}};


// ================= HTML CARD =================

cy.nodeHtmlLabel([
{{
    query: 'node',
    tpl: function(data) {{

        return `
<div style=""position:relative;width:220px;"">

    <div style=""background:#00aaff;color:white;padding:6px;border-radius:10px 10px 0 0;font-weight:bold;"">
        ` + data.code + `
    </div>

    <div style=""background:#f5f5f5;padding:8px;border-radius:0 0 10px 10px;"">

        <div style=""font-size:13px;"">
            ` + data.name + `
        </div>

        <div style=""display:flex;justify-content:space-between;font-size:11px;margin-top:6px;"">
            <span>ES: ` + data.es + `</span>
            <span>EF: ` + data.ef + `</span>
        </div>

    </div>

    <div onclick=""expandNode('` + data.id + `')"" 
         id=""btn_` + data.id + `"" 
         class=""expand-btn""
         style=""position:absolute;right:-12px;top:50%;transform:translateY(-50%);
                width:24px;height:24px;background:#2ecc71;color:white;
                border-radius:50%;text-align:center;line-height:24px;cursor:pointer;"">
        +
    </div>

</div>
        `;
    }}
}}
]);


// ================= SEND TO WPF =================

cy.on('tap', 'node', function(evt) {{
    var id = evt.target.id();

    if (window.chrome && window.chrome.webview) {{
        window.chrome.webview.postMessage(id);
    }}
}});

</script>

</body>
</html>";
        }
    }
}