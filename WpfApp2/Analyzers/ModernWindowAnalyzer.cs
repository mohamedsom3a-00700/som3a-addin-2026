using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Som3a_WPF_UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ModernWindowAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Usage";
        private const string DiagnosticId = "SWA001";
        private const string Title = "Use ModernWindow instead of Window";
        private const string MessageFormat = "Class '{0}' inherits from Window. Use ModernWindow instead for consistent Fluent design.";
        private const string Description = "All window classes should inherit from ModernWindow for unified Fluent design system.";

        public static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            if (!IsDerivedFromWindow(classDeclaration, context.SemanticModel))
                return;

            if (IsAlreadyModernWindow(classDeclaration, context.SemanticModel))
                return;

            var className = classDeclaration.Identifier.Text;
            var diagnostic = Diagnostic.Create(
                Rule,
                classDeclaration.Identifier.GetLocation(),
                className);

            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsDerivedFromWindow(ClassDeclarationSyntax classDecl, SemanticModel semanticModel)
        {
            var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);
            if (classSymbol == null)
                return false;

            var baseType = classSymbol.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == "Window" &&
                    (baseType.ContainingNamespace.Name == "System.Windows" ||
                     baseType.ContainingNamespace.Name == "PresentationFramework"))
                {
                    return true;
                }

                if (baseType.Name == "ModernWindow" &&
                    baseType.ContainingNamespace.Name == "Controls")
                {
                    return false;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }

        private static bool IsAlreadyModernWindow(ClassDeclarationSyntax classDecl, SemanticModel semanticModel)
        {
            var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);
            if (classSymbol == null)
                return false;

            var baseType = classSymbol.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == "ModernWindow" &&
                    baseType.ContainingNamespace.Name == "Controls" &&
                    baseType.ContainingNamespace.ContainingNamespace.Name == "Som3a_WPF_UI")
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class WindowStyleAnalyzer : DiagnosticAnalyzer
    {
        private const string Category = "Usage";
        private const string DiagnosticId = "SWA002";
        private const string Title = "Avoid AllowsTransparency=True";
        private const string MessageFormat = "Window '{0}' uses AllowsTransparency='True' which causes GPU/rendering issues. Use WindowChrome instead.";
        private const string Description = "Avoid AllowsTransparency=True for better rendering performance and VSTO stability.";

        public static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        }

        private void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            var attribute = (AttributeSyntax)context.Node;

            if (attribute.Name is IdentifierNameSyntax identifier &&
                identifier.Identifier.Text == "AllowsTransparency")
            {
                var windowParent = FindParentWindowDeclaration(attribute);
                if (windowParent != null)
                {
                    var windowName = windowParent.Identifier.Text;
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        attribute.GetLocation(),
                        windowName);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static ClassDeclarationSyntax? FindParentWindowDeclaration(SyntaxNode node)
        {
            var parent = node.Parent;
            while (parent != null)
            {
                if (parent is ClassDeclarationSyntax classDecl)
                {
                    return classDecl;
                }
                parent = parent.Parent;
            }
            return null;
        }
    }
}