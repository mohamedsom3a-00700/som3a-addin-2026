using Som3a.Contracts;

namespace Som3a.AI.Prompts
{
    public class RelationshipPrompt
    {
        public EnhancedPromptTemplate Template { get; }

        public RelationshipPrompt()
        {
            Template = new EnhancedPromptTemplate
            {
                Name = "Relationship Generator",
                Category = "Relationships",
                SystemPrompt = "You are a construction scheduling expert. Generate logical predecessor/successor relationships between construction activities based on trade sequence, space constraints, and resource flow.",
                UserPrompt = @"Given the following construction activities, generate logical predecessor/successor relationships.

Activities:
{{activities}}

Instructions:
1. For each pair of activities that have a dependency, create a relationship with:
   - predecessorId: Activity ID of the preceding activity
   - successorId: Activity ID of the succeeding activity
   - type: Relationship type (FS=Finish-to-Start, SS=Start-to-Start, FF=Finish-to-Finish, SF=Start-to-Finish)
   - lagDays: Lag value in days (positive = delay, negative = overlap, zero = no lag)
   - rationale: Brief explanation of why this dependency exists
   - confidence: High (clear trade sequence), Medium (probable), Low (weak heuristic)

2. Consider these dependency rules:
   - Trade sequence: Excavation → Foundation → Structure → Roof → MEP → Finishes
   - Space constraints: Activities in the same confined space cannot run concurrently
   - Resource flow: Activities sharing the same crew or equipment should be sequenced
   - Cross-trade: Rough-in work precedes finishes (e.g., MEP rough-in before drywall)

3. Output format: JSON array of relationship objects — NO wrapper object, NO markdown fences. Raw array only.
   Example: [{""predecessorId"":""A-001"",""successorId"":""A-002"",""type"":""FS"",""lagDays"":0,""rationale"":""Excavation precedes foundation"",""confidence"":""High""}]

4. At minimum, connect activities that are clearly sequenced by trade.
   Activities in different independent work streams may have no cross-relationships.",
                JsonSchema = @"{
                    ""type"": ""object"",
                    ""required"": [""relationships""],
                    ""properties"": {
                        ""relationships"": {
                            ""type"": ""array"",
                            ""items"": {
                                ""type"": ""object"",
                                ""required"": [""predecessorId"", ""successorId"", ""type""],
                                ""properties"": {
                                    ""predecessorId"": { ""type"": ""string"" },
                                    ""successorId"": { ""type"": ""string"" },
                                    ""type"": { ""type"": ""string"", ""enum"": [""FS"", ""SS"", ""FF"", ""SF""] },
                                    ""lagDays"": { ""type"": ""number"" },
                                    ""rationale"": { ""type"": ""string"" },
                                    ""confidence"": { ""type"": ""string"", ""enum"": [""High"", ""Medium"", ""Low""] }
                                }
                            }
                        }
                    }
                }",
                ContextRequirements = new() { "activities" },
                LifecycleState = TemplateLifecycleState.Draft,
                OwnershipScope = TemplateOwnership.System,
                CreatedBy = "system"
            };
        }

        public void Register(PromptTemplateRegistry registry)
        {
            var request = new CreateTemplateRequest(
                Name: Template.Name,
                Category: Template.Category,
                SystemPrompt: Template.SystemPrompt,
                UserPrompt: Template.UserPrompt,
                JsonSchema: Template.JsonSchema,
                ContextRequirements: Template.ContextRequirements,
                OwnershipScope: Template.OwnershipScope);

            registry.CreateAsync(request, "system").GetAwaiter().GetResult();
        }
    }
}
