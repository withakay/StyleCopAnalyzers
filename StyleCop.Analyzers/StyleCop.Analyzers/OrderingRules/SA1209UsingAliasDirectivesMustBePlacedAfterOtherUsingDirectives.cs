﻿namespace StyleCop.Analyzers.OrderingRules
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;


    /// <summary>
    /// A using-alias directive is positioned before a regular using directive.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs when a using-alias directive is placed before a normal using directive.
    /// Using-alias directives have special behavior which can alter the meaning of the rest of the code within the file
    /// or namespace. Placing the using-alias directives together below all other using-directives can make the code
    /// cleaner and easier to read, and can help make it easier to identify the types used throughout the code.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SA1209UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectives : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the
        /// <see cref="SA1209UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectives"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "SA1209";
        private const string Title = "Using alias directives must be placed after other using directives";
        private const string MessageFormat = "Using alias directive for '{0}' must appear after directive for '{1}'";
        private const string Category = "StyleCop.CSharp.OrderingRules";
        private const string Description = "A using-alias directive is positioned before a regular using directive.";
        private const string HelpLink = "http://www.stylecop.com/docs/SA1209.html";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, AnalyzerConstants.DisabledNoTests, Description, HelpLink);

        private static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnosticsValue =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return SupportedDiagnosticsValue;
            }
        }

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleCompilationUnit, SyntaxKind.CompilationUnit);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleNamespaceDeclaration, SyntaxKind.NamespaceDeclaration);
        }

        private void HandleCompilationUnit(SyntaxNodeAnalysisContext context)
        {
            var compilationUnit = context.Node as CompilationUnitSyntax;

            ProcessUsingsAndReportDiagnostic(compilationUnit.Usings, context);
        }

        private void HandleNamespaceDeclaration(SyntaxNodeAnalysisContext context)
        {
            var namespaceDeclaration = context.Node as NamespaceDeclarationSyntax;

            ProcessUsingsAndReportDiagnostic(namespaceDeclaration.Usings, context);
        }

        private static void ProcessUsingsAndReportDiagnostic(SyntaxList<UsingDirectiveSyntax> usings, SyntaxNodeAnalysisContext context)
        {
            UsingDirectiveSyntax usingAliasDirectivesShouldBePlacedAfterThis = null;
            var usingAliasDirectivesToReport = new Lazy<List<UsingDirectiveSyntax>>();

            for (int i = 0; i < usings.Count; i++)
            {
                var usingDirective = usings[i];
                var notLastUsingDirective = i + 1 < usings.Count;
                if (usingDirective.Alias != null && notLastUsingDirective)
                {
                    var nextUsingDirective = usings[i + 1];
                    if (nextUsingDirective.Alias == null && nextUsingDirective.StaticKeyword.IsKind(SyntaxKind.None))
                    {
                        usingAliasDirectivesToReport.Value.Add(usingDirective);
                    }
                }
                else
                {
                    usingAliasDirectivesShouldBePlacedAfterThis = usingDirective;
                }
            }

            if (usingAliasDirectivesToReport.IsValueCreated && usingAliasDirectivesToReport.Value.Count > 0)
            {
                var unaliasedNamespaceName = GetNamespaceNameWithoutAlias(usingAliasDirectivesShouldBePlacedAfterThis.Name.ToString());
                foreach (var usingAliasDirectiveToReport in usingAliasDirectivesToReport.Value)
                {                    
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, usingAliasDirectiveToReport.GetLocation(), usingAliasDirectiveToReport.Alias.Name.ToString(), unaliasedNamespaceName));
                }
            }
        }

        private static string GetNamespaceNameWithoutAlias(string name)
        {
            var result = name;
            int doubleColon = name.IndexOf("::");
            if (doubleColon >= 0)
            {
                result = name.Substring(doubleColon + 2);
            }

            return result;
        }
    }
}
