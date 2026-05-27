using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Som3a.Shared.Core;
using Som3a.Shared.Models;
using Som3a_WPF_UI.Properties;

namespace Som3a_WPF_UI.Services.WBS;

public class WBSTemplateService : IWBSTemplateService
{
    private readonly string _storagePath;
    private readonly List<WBSTemplate> _systemTemplates;
    private readonly Dictionary<string, List<string>> _categoryKeywords;

    public WBSTemplateService()
    {
        _storagePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Som3a", "wbs-templates");
        Directory.CreateDirectory(_storagePath);

        _systemTemplates = LoadSystemTemplates();
        _categoryKeywords = new Dictionary<string, List<string>>
        {
            ["Building"] = new() { "residential", "commercial", "industrial", "building", "office", "apartment", "warehouse", "retail", "hospitality", "hotel", "hospital" },
            ["Infrastructure"] = new() { "road", "bridge", "utility", "highway", "tunnel", "pipeline", "railway", "water", "wastewater", "drainage" },
            ["MEP"] = new() { "mechanical", "electrical", "plumbing", "hvac", "fire", "lighting", "power", "control" },
            ["Industrial"] = new() { "oil", "gas", "manufacturing", "petrochemical", "refinery", "factory", "plant", "processing" },
            ["Fitout"] = new() { "office fitout", "retail fitout", "hospitality fitout", "interior", "finish", "furnishing", "decoration" }
        };
    }

    public Task<List<WBSTemplateSummary>> ListTemplatesAsync(string? category = null)
    {
        var allTemplates = new List<WBSTemplate>(_systemTemplates);

        if (Directory.Exists(_storagePath))
        {
            foreach (var file in Directory.GetFiles(_storagePath, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var custom = JsonConvert.DeserializeObject<WBSTemplate>(json);
                    if (custom != null && !string.IsNullOrEmpty(custom.Id))
                        allTemplates.Add(custom);
                }
                catch { }
            }
        }

        var query = allTemplates.AsEnumerable();
        if (category != null)
            query = query.Where(t => t.Category == category);

        var summaries = query.Select(t => new WBSTemplateSummary(
            t.Id, t.Name, t.Category,
            t.RootNode != null ? GetMaxLevel(t.RootNode) : 0,
            t.RootNode != null ? CountNodes(t.RootNode) : 0,
            t.IsSystem
        )).ToList();

        return Task.FromResult(summaries);
    }

    public Task<WBSTemplate> GetTemplateAsync(string templateId)
    {
        var template = _systemTemplates.FirstOrDefault(t => t.Id == templateId);
        if (template != null)
            return Task.FromResult(template);

        var customPath = Path.Combine(_storagePath, $"{templateId}.json");
        if (File.Exists(customPath))
        {
            var json = File.ReadAllText(customPath);
            template = JsonConvert.DeserializeObject<WBSTemplate>(json);
            if (template != null)
                return Task.FromResult(template);
        }

        throw new KeyNotFoundException($"Template '{templateId}' not found.");
    }

    public Task<WBSTemplate> CreateCustomTemplateAsync(string name, string category, WBSNode rootNode, string userId)
    {
        var template = new WBSTemplate
        {
            Name = name, Category = category, RootNode = rootNode,
            IsSystem = false, OwnerId = userId, Version = 1
        };
        template.Validate();
        var path = Path.Combine(_storagePath, $"{template.Id}.json");
        var json = JsonConvert.SerializeObject(template, Formatting.Indented);
        File.WriteAllText(path, json);
        return Task.FromResult(template);
    }

    public Task ImportTemplateAsync(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var template = JsonConvert.DeserializeObject<WBSTemplate>(json);
        if (template == null) throw new InvalidDataException("Invalid template file.");
        template.Validate();
        var destPath = Path.Combine(_storagePath, $"{template.Id}.json");
        File.Copy(filePath, destPath, overwrite: true);
        return Task.CompletedTask;
    }

    public Task ExportTemplateAsync(string templateId, string filePath)
    {
        var template = _systemTemplates.FirstOrDefault(t => t.Id == templateId);
        if (template == null)
        {
            var customPath = Path.Combine(_storagePath, $"{templateId}.json");
            if (!File.Exists(customPath))
                throw new KeyNotFoundException($"Template '{templateId}' not found.");
            File.Copy(customPath, filePath, overwrite: true);
            return Task.CompletedTask;
        }
        var json = JsonConvert.SerializeObject(template, Formatting.Indented);
        File.WriteAllText(filePath, json);
        return Task.CompletedTask;
    }

    public List<WBSTemplate> GetRecommendedTemplates(string projectDescription)
    {
        if (string.IsNullOrWhiteSpace(projectDescription))
            return _systemTemplates.Take(3).ToList();

        var lower = projectDescription.ToLower();
        var scores = new Dictionary<string, int>();
        foreach (var kvp in _categoryKeywords)
        {
            var score = kvp.Value.Count(kw => lower.Contains(kw));
            if (score > 0) scores[kvp.Key] = score;
        }

        var bestCategory = scores.OrderByDescending(s => s.Value).FirstOrDefault().Key;
        if (bestCategory == null) return _systemTemplates.Take(3).ToList();
        return _systemTemplates.Where(t => t.Category == bestCategory).ToList();
    }

    private static List<WBSTemplate> LoadSystemTemplates()
    {
        return new List<WBSTemplate>
        {
            // === 6-Level Default Project Template ===
            new() { Name = "Default Construction WBS", Category = "General",
                Description = "Standard 6-level project WBS: Milestones, Engineering, Procurement, Construction, T&C, Handover",
                RootNode = BuildDefaultProjectTree(), IsSystem = true },

            // === Building — Residential ===
            new() { Name = "Building — Residential", Category = "Building",
                Description = "Residential buildings, apartments, villas — construction sequence WBS (8 levels)",
                RootNode = BuildBuildingTree("Residential Building"), IsSystem = true },

            // === Building — Commercial ===
            new() { Name = "Building — Commercial", Category = "Building",
                Description = "Office buildings, malls, hotels — construction sequence WBS (8 levels)",
                RootNode = BuildBuildingTree("Commercial Building"), IsSystem = true },

            // === Building — Industrial ===
            new() { Name = "Building — Industrial", Category = "Building",
                Description = "Factories, warehouses, plants — construction sequence WBS (8 levels)",
                RootNode = BuildBuildingTree("Industrial Building"), IsSystem = true },

            // === Infrastructure ===
            new() { Name = "Infrastructure", Category = "Infrastructure",
                Description = "Roads, bridges, utilities — sequence-based WBS covering all asset types (9 levels)",
                RootNode = BuildInfrastructureTree(), IsSystem = true },

            // === MEP ===
            new() { Name = "MEP Works", Category = "MEP",
                Description = "Mechanical, Electrical, Plumbing, Fire Protection, ELV — standalone MEP WBS",
                RootNode = BuildMEPTree(), IsSystem = true },

            // === Industrial ===
            new() { Name = "Industrial — Oil & Gas / Manufacturing", Category = "Industrial",
                Description = "Civil, Structural Steel, Mechanical Equipment, Piping, E&I, Coating — industrial WBS",
                RootNode = BuildIndustrialTree(), IsSystem = true },

            // === Fitout ===
            new() { Name = "Fitout — Office / Retail / Hospitality", Category = "Fitout",
                Description = "Demolition, Partitions, Ceilings, Flooring, Joinery, Finishes, MEP Fitout, Handover",
                RootNode = BuildFitoutTree(), IsSystem = true },
        };
    }

    // ========== Default Project (6-Level) ==========
    private static WBSNode BuildDefaultProjectTree()
    {
        var root = new WBSNode { Code = "1", Name = "Project" };
        AddChild(root, "Milestones",
            AddChild("Project Commencement"),
            AddChild("Section Milestones", "Design Milestone", "Procurement Milestone", "Construction Milestone", "Handover Milestone"),
            AddChild("Project Completion"));

        AddChild(root, "Engineering",
            AddChild("Structural Engineering", "Shop Drawings", "Design Submittals", "Approvals"),
            AddChild("Architectural", "Shop Drawings", "Material Submittals", "Approvals"),
            AddChild("MEP Engineering", "Shop Drawings", "Calculations", "Approvals"),
            AddChild("Civil Engineering", "Design", "Submittals"));

        AddChild(root, "Procurement",
            AddChild("Long Lead Items", "Submittals", "Approvals", "Purchase Order", "Manufacturing", "Delivery"),
            AddChild("Bulk Materials", "Submittals", "Approvals", "Purchase Order", "Delivery"),
            AddChild("Equipment", "Technical Submittals", "Approvals", "Purchase Order", "Fabrication", "Delivery"));

        AddChild(root, "Construction",
            AddChild("Site Preparation"),
            AddChild("Building Works"),
            AddChild("Infrastructure Works"),
            AddChild("MEP Installation"));

        AddChild(root, "Testing & Commissioning",
            AddChild("System Testing"),
            AddChild("Integration Testing"),
            AddChild("Commissioning"),
            AddChild("Performance Verification"));

        AddChild(root, "Handover",
            AddChild("Documentation", "As-built Drawings", "O&M Manuals", "Test Reports"),
            AddChild("Training", "Operator Training", "Maintenance Training"),
            AddChild("Final Handover", "Snagging", "Punch List", "Certificate of Completion"));

        NumberNodes(root);
        return root;
    }

    // ========== Building Tree (Residential / Commercial / Industrial) ==========
    private static WBSNode BuildBuildingTree(string projectType)
    {
        var root = new WBSNode { Code = "1", Name = projectType };
        AddChild(root, "Site Preparation & Earthworks",
            AddChild("Site Clearing", "Demolition", "Grubbing", "Topsoil Stripping"),
            AddChild("Excavation", "Bulk Excavation", "Trench Excavation", "Foundation Excavation"),
            AddChild("Backfilling & Compaction", "Select Fill", "General Fill", "Compaction Testing"),
            AddChild("Dewatering", "Surface Water Control", "Groundwater Lowering"),
            AddChild("Temporary Works", "Site Fencing", "Temporary Access Roads", "Welfare Facilities"));

        AddChild(root, "Substructure",
            AddChild("Foundations", "Strip Footings", "Pad Footings", "Raft Foundation", "Pile Caps", "Ground Beams"),
            AddChild("Basement (if applicable)", "Basement Excavation", "Waterproofing", "Basement Slab", "Basement Walls"),
            AddChild("Ground Floor Slab", "Hardcore", "Blinding", "Reinforcement", "Concrete Pour", "Curing"));

        AddChild(root, "Superstructure",
            AddChild("Columns", "Reinforcement", "Formwork", "Concrete Pour"),
            AddChild("Beams", "Reinforcement", "Formwork", "Concrete Pour"),
            AddChild("Slabs", "Reinforcement", "Formwork", "Concrete Pour"),
            AddChild("Shear Walls / Core", "Reinforcement", "Formwork", "Concrete Pour"),
            AddChild("Staircases", "Reinforcement", "Formwork", "Concrete Pour"),
            AddChild("Masonry Walls", "Blockwork", "Reinforcement", "Damp Proof Course"));

        AddChild(root, "Building Envelope",
            AddChild("External Walls", "Cladding Installation", "Insulation", "Vapor Barrier"),
            AddChild("Roofing", "Roof Structure", "Insulation", "Waterproofing", "Roof Finishes"),
            AddChild("Windows & Glazing", "Frame Installation", "Glass Installation", "Sealing"),
            AddChild("External Doors", "Frame Installation", "Door Installation", "Hardware"));

        AddChild(root, "Internal Finishes",
            AddChild("Plastering & Rendering", "Internal Plaster", "External Render", "Cornices"),
            AddChild("Flooring", "Screed", "Tiling", "Vinyl Flooring", "Carpet", "Wood Flooring"),
            AddChild("Ceilings", "Suspended Ceiling Grid", "Ceiling Tiles", "Plasterboard Ceiling"),
            AddChild("Painting & Decoration", "Primer", "Emulsion Paint", "Oil Paint", "Wallpaper"),
            AddChild("Doors & Hardware", "Internal Doors", "Ironmongery", "Door Closers"),
            AddChild("Sanitary Ware", "WC Installation", "Sinks", "Showers", "Accessories"));

        AddChild(root, "MEP Works",
            AddChild("Mechanical (HVAC)", "Ductwork", "AHU Installation", "FCU Installation", "Insulation", "Testing"),
            AddChild("Electrical", "Cable Trays", "Cabling", "Panel Installation", "Lighting", "Power Outlets", "Earthing"),
            AddChild("Plumbing", "Pipework", "Drainage", "Water Tanks", "Pumps", "Fixtures"),
            AddChild("Fire Protection", "Sprinkler System", "Fire Alarms", "Fire Extinguishers", "Smoke Detectors"),
            AddChild("Low Current / ELV", "Data Cabling", "CCTV", "Access Control", "BMS", "Telephone"));

        AddChild(root, "External Works",
            AddChild("Landscaping", "Topsoil", "Planting", "Irrigation", "Lawn"),
            AddChild("Paving & Roads", "Sub-base", "Paving", "Kerbs", "Asphalt"),
            AddChild("Fencing & Gates", "Boundary Fencing", "Gates", "Access Control"),
            AddChild("Utilities Connection", "Water Connection", "Sewer Connection", "Electrical Connection"));

        AddChild(root, "Testing & Commissioning",
            AddChild("MEP Commissioning", "HVAC Balancing", "Electrical Testing", "Plumbing Testing"),
            AddChild("Building Systems Testing", "Fire Alarm Test", "BMS Integration", "Emergency Lighting Test"),
            AddChild("Snagging & Handover", "Snag List", "Defect Rectification", "Final Inspection"));

        NumberNodes(root);
        return root;
    }

    private static WBSNode BuildInfrastructureTree()
    {
        var root = new WBSNode { Code = "1", Name = "Infrastructure" };
        AddChild(root, "Site Preparation",
            AddChild("Survey & Setting Out", "Topographic Survey", "Control Points", "Route Setting Out"),
            AddChild("Site Clearance", "Vegetation Clearance", "Obstruction Removal", "Topsoil Stripping"),
            AddChild("Temporary Works", "Access Roads", "Laydown Areas", "Traffic Diversions"),
            AddChild("Traffic Management", "Traffic Control Plan", "Signage", "Barriers"));

        AddChild(root, "Earthworks",
            AddChild("Cut & Fill", "Excavation (Cut)", "Embankment Fill", "Compaction"),
            AddChild("Embankment Construction", "Layer Placement", "Compaction", "Slope Formation"),
            AddChild("Slope Protection", "Gabions", "Rip Rap", "Soil Nails", "Geotextiles"),
            AddChild("Compaction Testing", "Field Density Tests", "Laboratory Tests"));

        AddChild(root, "Drainage",
            AddChild("Surface Drainage", "Open Channels", "Kerbs & Gutters", "Catch Basins"),
            AddChild("Subsurface Drainage", "Perforated Pipes", "Filter Fabric", "Outfalls"),
            AddChild("Culverts", "Box Culverts", "Pipe Culverts", "Headwalls", "Aprons"),
            AddChild("Retention Ponds", "Excavation", "Lining", "Outlet Structure"));

        AddChild(root, "Pavement / Road Structure",
            AddChild("Sub-base", "Granular Sub-base", "Placement", "Compaction"),
            AddChild("Base Course", "Aggregate Base", "Cement-treated Base", "Compaction"),
            AddChild("Wearing Course", "Asphalt Concrete", "Concrete Pavement", "Surface Treatment"),
            AddChild("Kerbs & Channels", "Precast Kerbs", "Cast-in-place Kerbs", "Channels"),
            AddChild("Sidewalks", "Base Preparation", "Paving", "Accessibility Features"));

        AddChild(root, "Bridges & Structures",
            AddChild("Foundation", "Pile Driving", "Pile Caps", "Spread Footings"),
            AddChild("Substructure", "Abutments", "Pier Columns", "Pier Caps"),
            AddChild("Superstructure", "Girders / Beams", "Deck Slab", "Parapets"),
            AddChild("Bridge Deck & Wearing Surface", "Waterproofing", "Asphalt Overlay", "Expansion Joints"),
            AddChild("Bearings & Joints", "Elastomeric Bearings", "Expansion Joints", "Drainage"));

        AddChild(root, "Utilities",
            AddChild("Water Supply", "Pipe Laying", "Valves & Fittings", "Hydrants", "Testing & Disinfection"),
            AddChild("Sewerage", "Pipe Laying", "Manholes", "Rising Mains", "Testing"),
            AddChild("Electrical Ducts", "Trenching", "Duct Installation", "Handholes"),
            AddChild("Telecom Ducts", "Trenching", "Duct Installation", "Manholes"),
            AddChild("Gas Mains", "Pipe Laying", "Welding", "Testing", "Valve Stations"));

        AddChild(root, "Traffic & Safety",
            AddChild("Signage", "Sign Posts", "Sign Panels", "Overhead Signs"),
            AddChild("Road Markings", "Thermoplastic Markings", "Paint Markings", "Road Studs"),
            AddChild("Guardrails & Barriers", "Metal Guardrails", "Concrete Barriers", "Terminal Ends"),
            AddChild("Street Lighting", "Foundation", "Poles", "Luminaires", "Cabling", "Control Panels"),
            AddChild("Traffic Signals", "Poles", "Signal Heads", "Controllers", "Loop Detectors"));

        AddChild(root, "Landscaping & Finishes",
            AddChild("Topsoil & Seeding", "Topsoil Placement", "Seeding", "Mulching"),
            AddChild("Tree Planting", "Tree Pits", "Trees", "Staking & Guying"),
            AddChild("Irrigation", "Pipe Network", "Sprinklers", "Controllers"),
            AddChild("Fencing", "Chain Link Fencing", "Security Fencing", "Gates"));

        AddChild(root, "Testing & Handover",
            AddChild("Material Testing", "Concrete Tests", "Asphalt Tests", "Soil Tests"),
            AddChild("Load Testing", "Bridge Load Test", "Proof Testing"),
            AddChild("As-built Documentation", "Record Drawings", "Test Reports", "O&M Manuals"));

        NumberNodes(root);
        return root;
    }

    private static WBSNode BuildMEPTree()
    {
        var root = new WBSNode { Code = "1", Name = "MEP Works" };
        AddChild(root, "Mechanical (HVAC)",
            AddChild("Ductwork", "Supply Ductwork", "Return Ductwork", "Insulation", "Diffusers & Grilles"),
            AddChild("Air Handling Units", "AHU Installation", "FCU Installation", "Condenser Units"),
            AddChild("Piping", "Chilled Water Pipes", "Condenser Water Pipes", "Insulation"),
            AddChild("Controls & BMS", "Thermostats", "Sensors", "Control Panels", "Integration Testing"));

        AddChild(root, "Electrical",
            AddChild("Power Distribution", "Main Switchboard", "Sub-distribution Boards", "Cable Trays", "Cabling"),
            AddChild("Lighting", "Light Fixtures", "Emergency Lighting", "Lighting Control"),
            AddChild("Earthing & Bonding", "Earth Electrode", "Bonding Conductors", "Testing"),
            AddChild("Standby Power", "Generator", "ATS", "Fuel System"),
            AddChild("UPS System", "UPS Installation", "Battery Bank", "Bypass"));

        AddChild(root, "Plumbing",
            AddChild("Water Supply", "Mains Connection", "Rising Mains", "Distribution Pipework"),
            AddChild("Drainage", "Soil & Waste Pipes", "Vent Pipes", "Floor Drains"),
            AddChild("Plumbing Fixtures", "WC's", "Sinks", "Urinals", "Showers"),
            AddChild("Water Heating", "Calorifiers", "Water Heaters", "Circulation Pumps"));

        AddChild(root, "Fire Protection",
            AddChild("Sprinkler System", "Pipe Network", "Sprinkler Heads", "Valves", "Pumps"),
            AddChild("Fire Alarm System", "Detectors", "Alarm Panels", "Bells & Strobes"),
            AddChild("Fire Extinguishers", "Extinguisher Installation", "Hose Reels", "Fire Blankets"),
            AddChild("Smoke Control", "Smoke Extract Fans", "Dampers", "Controls"));

        AddChild(root, "Low Current / ELV",
            AddChild("Data & Telecom", "Cabling", "Patch Panels", "Outlets"),
            AddChild("CCTV System", "Cameras", "DVR/NVR", "Cabling"),
            AddChild("Access Control", "Card Readers", "Door Controllers", "Software"),
            AddChild("Building Management System", "BMS Panel", "Field Devices", "Integration"));

        AddChild(root, "Testing & Commissioning",
            AddChild("Pre-commissioning", "Megger Testing", "Continuity Testing", "Pressure Testing"),
            AddChild("Commissioning", "System Start-up", "Setpoints", "Functional Testing"),
            AddChild("Performance Verification", "Air Balancing", "Power Quality", "BMS Integration Test"));

        NumberNodes(root);
        return root;
    }

    private static WBSNode BuildIndustrialTree()
    {
        var root = new WBSNode { Code = "1", Name = "Industrial — Oil & Gas / Manufacturing" };
        AddChild(root, "Civil Works",
            AddChild("Site Preparation", "Clearing", "Grading", "Compaction"),
            AddChild("Foundations", "Equipment Foundations", "Structural Foundations", "Pile Caps"),
            AddChild("Industrial Slabs", "Heavy Duty Slabs", "Light Duty Slabs", "Surface Hardening"),
            AddChild("Roads & Paving", "Internal Roads", "Laydown Areas", "Parking"),
            AddChild("Drainage", "Process Drainage", "Stormwater Drainage", "Oil-water Separators"));

        AddChild(root, "Structural Steel",
            AddChild("Columns", "Steel Columns", "Base Plates", "Grouting"),
            AddChild("Trusses & Roofs", "Roof Trusses", "Purlins", "Bracing"),
            AddChild("Platforms & Walkways", "Steel Platforms", "Handrails", "Staircases"),
            AddChild("Cladding", "Wall Cladding", "Roof Cladding", "Insulation"));

        AddChild(root, "Mechanical Equipment",
            AddChild("Vessels & Tanks", "Storage Tanks", "Pressure Vessels", "Agitators"),
            AddChild("Pumps & Compressors", "Centrifugal Pumps", "Reciprocating Compressors", "Drivers"),
            AddChild("Conveyors & Material Handling", "Belt Conveyors", "Screw Conveyors", "Bucket Elevators"),
            AddChild("Packaged Equipment", "Package Boilers", "Chillers", "Air Compressors"));

        AddChild(root, "Piping",
            AddChild("Process Piping", "Pipe Spools", "Welding", "Supports", "Valves"),
            AddChild("Utility Piping", "Cooling Water", "Steam", "Compressed Air"),
            AddChild("Pipe Supports & Hangers", "Structural Supports", "Spring Hangers", "Guides"),
            AddChild("Insulation", "Hot Insulation", "Cold Insulation", "Cladding"));

        AddChild(root, "Electrical & Instrumentation",
            AddChild("Power Distribution", "Substation", "MCC", "Cabling", "Earthing"),
            AddChild("Controls & DCS", "DCS Panels", "PLC Panels", "Field Instruments"),
            AddChild("Cabling & Raceways", "Cable Trays", "Conduits", "Instrument Cables", "Power Cables"),
            AddChild("Lighting & Small Power", "Area Lighting", "Local Panels", "Sockets"));

        AddChild(root, "Painting, Coating & Insulation",
            AddChild("Surface Preparation", "Abrasive Blasting", "Primer", "Surface Profile"),
            AddChild("Coating Systems", "Epoxy Coating", "Polyurethane", "Intumescent"),
            AddChild("Fireproofing", "Cementitious Fireproofing", "Intumescent Paint"));

        AddChild(root, "Testing & Pre-commissioning",
            AddChild("Hydrostatic Testing", "Pipework Test", "Vessel Test"),
            AddChild("Electrical Testing", "Megger Test", "Hi-Pot Test", "Protection Relay Testing"),
            AddChild("Instrument Loop Check", "Continuity Check", "Calibration", "Functional Test"),
            AddChild("Pre-commissioning & Start-up", "System Dry-out", "Flushing", "Commissioning"));

        NumberNodes(root);
        return root;
    }

    private static WBSNode BuildFitoutTree()
    {
        var root = new WBSNode { Code = "1", Name = "Fitout — Office / Retail / Hospitality" };
        AddChild(root, "Demolition & Preparation",
            AddChild("Soft Strip", "Ceiling Removal", "Partition Removal", "Floor Covering Removal"),
            AddChild("Hard Demolition", "Wall Removal", "Structural Modifications"),
            AddChild("Site Preparation", "Cleaning", "Protection of Existing Works"));

        AddChild(root, "Partitioning & Drywall",
            AddChild("Metal Stud Partitions", "Track Installation", "Stud Installation", "Boarding"),
            AddChild("Glass Partitions", "Frame Installation", "Glass Panels", "Doors"),
            AddChild("Acoustic Partitions", "Acoustic Insulation", "Double Boarding", "Sealing"));

        AddChild(root, "Ceilings",
            AddChild("Suspended Ceiling Grid", "Grid Installation", "Ceiling Tiles", "Edge Trim"),
            AddChild("Plasterboard Ceiling", "Metal Framing", "Boarding", "Jointing"),
            AddChild("Feature Ceilings", "Timber Feature", "Metal Panels", "Lighting Integration"));

        AddChild(root, "Flooring",
            AddChild("Screed & Leveling", "Sand-cement Screed", "Self-leveling Compound"),
            AddChild("Tiling", "Ceramic Tiles", "Porcelain Tiles", "Stone Flooring"),
            AddChild("Carpet", "Underlay", "Carpet Tiles", "Broadloom Carpet"),
            AddChild("Vinyl & LVT", "Vinyl Sheeting", "LVT Planks", "Welding"),
            AddChild("Wood Flooring", "Engineered Wood", "Laminate", "Finishing"));

        AddChild(root, "Joinery & Carpentry",
            AddChild("Built-in Furniture", "Reception Desk", "Workstations", "Cabinets"),
            AddChild("Wardrobes & Storage", "Wardrobe Installation", "Shelving", "Drawers"),
            AddChild("Wall Paneling", "Timber Paneling", "Acoustic Panels", "Feature Walls"));

        AddChild(root, "Finishes",
            AddChild("Painting", "Primer", "Emulsion", "Gloss", "Feature Paint"),
            AddChild("Wallpaper & Wall Coverings", "Wallpaper Installation", "Vinyl Wall Coverings", "Fabric"),
            AddChild("Cladding & Feature Finishes", "Stone Cladding", "Tile Feature", "Metal Finishes"));

        AddChild(root, "MEP Fitout",
            AddChild("Lighting Installation", "Downlights", "Track Lighting", "Decorative Lighting", "Emergency Lighting"),
            AddChild("Power & Data", "Floor Boxes", "Wall Sockets", "Data Outlets", "Cabling"),
            AddChild("HVAC Diffusers & Grilles", "Supply Diffusers", "Return Grilles", "Linear Slots"),
            AddChild("Sanitary & Plumbing", "Toilets", "Sinks", "Kitchenette Plumbing"));

        AddChild(root, "Handover & Snagging",
            AddChild("Cleaning", "Final Clean", "Window Cleaning", "Waste Removal"),
            AddChild("Snagging", "Snag List Preparation", "Defect Rectification", "Re-inspection"),
            AddChild("Handover", "Key Handover", "Documentation", "Training"));

        NumberNodes(root);
        return root;
    }

    // ========== Helpers ==========
    private static WBSNode AddChild(string name, params string[] grandchildren)
    {
        var node = new WBSNode { Name = name };
        for (int i = 0; i < grandchildren.Length; i++)
        {
            var gc = new WBSNode { Name = grandchildren[i] };
            gc.Parent = node;
            node.Children.Add(gc);
        }
        return node;
    }

    private static void AddChild(WBSNode parent, string name, params WBSNode[] children)
    {
        var node = new WBSNode { Name = name };
        foreach (var c in children)
        {
            c.Parent = node;
            node.Children.Add(c);
        }
        node.Parent = parent;
        parent.Children.Add(node);
    }

    private static void NumberNodes(WBSNode root)
    {
        int index = 1;
        NumberNodeRecursive(root, ref index, null);
    }

    private static void NumberNodeRecursive(WBSNode node, ref int index, string? parentCode)
    {
        node.Code = parentCode == null ? index.ToString() : $"{parentCode}.{index}";
        int childIdx = 1;
        foreach (var child in node.Children)
            NumberNodeRecursive(child, ref childIdx, node.Code);
        index++;
    }

    private static int GetMaxLevel(WBSNode node) => node.Children.Count == 0 ? 1 : 1 + node.Children.Max(GetMaxLevel);
    private static int CountNodes(WBSNode node) => 1 + node.Children.Sum(CountNodes);

    public Task ExportTemplateToExcelAsync(string templateId, object excelApp)
    {
        var template = _systemTemplates.FirstOrDefault(t => t.Id == templateId)
            ?? throw new KeyNotFoundException($"Template '{templateId}' not found.");

        var app = (Microsoft.Office.Interop.Excel.Application)excelApp;
        var workbook = app.Workbooks.Add();
        var sheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];
        var sheetName = $"WBS - {template.Name}";
        if (sheetName.Length > 31) sheetName = sheetName.Substring(0, 31);
        sheet.Name = sheetName;

        sheet.Cells[1, 1] = "Code";
        sheet.Cells[1, 2] = "Name";
        sheet.Cells[1, 3] = "Level";
        sheet.Cells[1, 4] = "ParentCode";
        sheet.Cells[1, 5] = "FullPath";

        var header = sheet.Range[sheet.Cells[1, 1], sheet.Cells[1, 5]];
        header.Font.Bold = true;

        var styleName = Settings.Default.WBSExportStyle;
        var styleId = styleName switch
        {
            "Blue Gradient" => 2,
            "Primavera" => 3,
            "Dark Mode" => 4,
            "Soft Pastel" => 5,
            _ => 1
        };
        var levelStyleMap = WbsStyleFactory.GetStyle(styleId);

        int row = 2;
        if (template.RootNode != null)
            WriteTemplateNodeWithStyle(sheet, template.RootNode, null, ref row, levelStyleMap);

        sheet.Columns.AutoFit();
        return Task.CompletedTask;
    }

    private static void WriteTemplateNodeWithStyle(Microsoft.Office.Interop.Excel.Worksheet sheet, WBSNode node, WBSNode? parent, ref int row, IReadOnlyDictionary<int, WbsLevelStyle> levelStyleMap)
    {
        sheet.Cells[row, 1] = node.Code;
        sheet.Cells[row, 2] = node.Name;
        sheet.Cells[row, 3] = node.Level;
        sheet.Cells[row, 4] = parent?.Code ?? "";
        sheet.Cells[row, 5] = node.FullPath;

        var styleKey = node.Level + 1;
        if (levelStyleMap.TryGetValue(styleKey, out var levelStyle))
        {
            var rowRange = sheet.Range[sheet.Cells[row, 1], sheet.Cells[row, 5]];
            rowRange.Interior.Color = levelStyle.Fill;
            rowRange.Font.Color = levelStyle.Font;
        }

        row++;
        foreach (var child in node.Children)
            WriteTemplateNodeWithStyle(sheet, child, node, ref row, levelStyleMap);
    }

    private static void WriteTemplateNode(Microsoft.Office.Interop.Excel.Worksheet sheet, WBSNode node, WBSNode? parent, ref int row)
    {
        sheet.Cells[row, 1] = node.Code;
        sheet.Cells[row, 2] = node.Name;
        sheet.Cells[row, 3] = node.Level;
        sheet.Cells[row, 4] = parent?.Code ?? "";
        sheet.Cells[row, 5] = node.FullPath;
        row++;
        foreach (var child in node.Children)
            WriteTemplateNode(sheet, child, node, ref row);
    }

    public Task<WBSTemplate> ImportTemplateFromExcelAsync(object excelApp, string? sheetName = null, string? category = null)
    {
        var app = (Microsoft.Office.Interop.Excel.Application)excelApp;
        var sheet = sheetName != null
            ? (Microsoft.Office.Interop.Excel.Worksheet)app.ActiveWorkbook.Sheets[sheetName]
            : (Microsoft.Office.Interop.Excel.Worksheet)app.ActiveSheet;

        var used = sheet.UsedRange;
        if (used == null || used.Rows.Count < 2)
            throw new InvalidDataException("No data found in the selected sheet.");

        var template = new WBSTemplate
        {
            Name = sheet.Name?.Replace("WBS Template - ", "") ?? "Imported Template",
            Category = category ?? "Custom",
            IsSystem = false,
            RootNode = new WBSNode { Code = "1", Name = "Root" }
        };

        var nodeMap = new Dictionary<string, WBSNode> { ["1"] = template.RootNode };

        for (int r = 2; r <= used.Rows.Count; r++)
        {
            var code = (used.Cells[r, 1] as Microsoft.Office.Interop.Excel.Range)?.Value?.ToString() ?? "";
            var name = (used.Cells[r, 2] as Microsoft.Office.Interop.Excel.Range)?.Value?.ToString() ?? "";
            var parentCode = (used.Cells[r, 4] as Microsoft.Office.Interop.Excel.Range)?.Value?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(code) || code == "1") continue;

            var node = new WBSNode { Code = code, Name = name };

            WBSNode? parentNode = null;
            if (!string.IsNullOrWhiteSpace(parentCode))
                nodeMap.TryGetValue(parentCode, out parentNode);

            if (parentNode != null)
            {
                node.Parent = parentNode;
                parentNode.Children.Add(node);
            }
            else if (template.RootNode != null)
            {
                node.Parent = template.RootNode;
                template.RootNode.Children.Add(node);
            }

            nodeMap[code] = node;
        }

        template.Validate();
        var path = Path.Combine(_storagePath, $"{template.Id}.json");
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(template, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(path, json);

        return Task.FromResult(template);
    }
}
