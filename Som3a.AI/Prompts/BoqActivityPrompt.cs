using Som3a.Contracts;

namespace Som3a.AI.Prompts
{
    public class BoqActivityPrompt
    {
        public EnhancedPromptTemplate Template { get; }

        public BoqActivityPrompt()
        {
            Template = new EnhancedPromptTemplate
            {
                Name = "BOQ to Activities",
                Category = "BOQ",
                SystemPrompt = "You are a construction planning assistant. Generate structured construction activities from BOQ items. Each activity must have a verb-noun name, description, BOQ reference, quantity, and unit.",
                UserPrompt = @"Given the following BOQ data, generate construction activities.

BOQ Context:
- Workbook: {{workbookName}}
- Sheet: {{sheetName}}
- Total Items: {{itemCount}}
- Total Quantity: {{totalQuantity}}
- Truncated: {{isTruncated}}

BOQ Items:
{{boqItems}}

Instructions:
1. Group similar BOQ items into single activities where appropriate
2. Use verb-noun naming convention (e.g., ""Pour Concrete Foundation"", ""Install Rebar"")
3. Each activity must reference its source BOQ items
4. Assign a trade category to each activity
5. Provide quantities and units for each activity
6. Output as a structured list",
                JsonSchema = @"{
                    ""type"": ""array"",
                    ""items"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""name"": { ""type"": ""string"" },
                            ""description"": { ""type"": ""string"" },
                            ""boqReferences"": { ""type"": ""array"", ""items"": { ""type"": ""string"" } },
                            ""quantity"": { ""type"": ""number"" },
                            ""unit"": { ""type"": ""string"" },
                            ""tradeCategory"": { ""type"": ""string"" },
                            ""wbsPath"": { ""type"": ""string"" }
                        }
                    }
                }",
                ContextRequirements = new() { "workbookName", "sheetName", "itemCount", "totalQuantity", "isTruncated", "boqItems" },
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
