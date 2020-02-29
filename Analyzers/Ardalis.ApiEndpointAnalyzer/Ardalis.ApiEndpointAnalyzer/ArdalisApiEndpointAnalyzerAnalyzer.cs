using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Ardalis.ApiEndpointAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ArdalisApiEndpointAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ArdalisApiEndpointAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Best Practice";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {

            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            var typeSymbol = context.Compilation.GetTypeByMetadataName("RickHodder.ApiEndpoint");

            if (typeSymbol == null)
                return;

            if (InheritsFrom(namedTypeSymbol, typeSymbol))
            {
                var members = namedTypeSymbol.GetMembers();

                var methods = members.Where(m => m.Kind == SymbolKind.Method && ((m as IMethodSymbol)?.MethodKind == MethodKind.Ordinary));

                if (methods.Count() != 1)
                {
                    var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        // got InheritsFrom from:
        // https://www.meziantou.net/working-with-types-in-a-roslyn-analyzer.htm#checking-a-type-inhe

        private static bool InheritsFrom(INamedTypeSymbol symbol, ITypeSymbol type)
        {
            var baseType = symbol.BaseType;
            while (baseType != null)
            {
                if (type.Equals(baseType))
                    return true;

                baseType = baseType.BaseType;
            }

            return false;
        }

    }
}
