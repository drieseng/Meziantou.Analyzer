﻿using System;
using System.Collections.Immutable;
using Meziantou.Analyzer.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Meziantou.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CommaAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            RuleIdentifiers.MissingCommaInObjectInitializer,
            title: "Add comma after the last property",
            messageFormat: "Add comma after the last property",
            RuleCategories.Style,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "",
            helpLinkUri: RuleIdentifiers.GetHelpUri(RuleIdentifiers.MissingCommaInObjectInitializer));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_rule);

        private static readonly Action<SyntaxNodeAnalysisContext> s_handleObjectInitializerAction = HandleObjectInitializer;
        private static readonly Action<SyntaxNodeAnalysisContext> s_handleAnonymousObjectInitializerAction = HandleAnonymousObjectInitializer;
        private static readonly Action<SyntaxNodeAnalysisContext> s_handleEnumDeclarationAction = HandleEnumDeclaration;

        private static readonly ImmutableArray<SyntaxKind> ObjectInitializerKinds =
            ImmutableArray.Create(SyntaxKind.ObjectInitializerExpression, SyntaxKind.ArrayInitializerExpression, SyntaxKind.CollectionInitializerExpression);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(s_handleObjectInitializerAction, ObjectInitializerKinds);
            context.RegisterSyntaxNodeAction(s_handleAnonymousObjectInitializerAction, SyntaxKind.AnonymousObjectCreationExpression);
            context.RegisterSyntaxNodeAction(s_handleEnumDeclarationAction, SyntaxKind.EnumDeclaration);
        }

        private static void HandleEnumDeclaration(SyntaxNodeAnalysisContext context)
        {
            var initializer = (EnumDeclarationSyntax)context.Node;
            var lastMember = initializer.Members.LastOrDefault();
            if (lastMember == null || !initializer.SpansMultipleLines())
            {
                return;
            }

            if (initializer.Members.Count != initializer.Members.SeparatorCount)
            {
                context.ReportDiagnostic(Diagnostic.Create(s_rule, lastMember.GetLocation()));
            }
        }

        private static void HandleObjectInitializer(SyntaxNodeAnalysisContext context)
        {
            var initializer = (InitializerExpressionSyntax)context.Node;
            if (initializer == null || !initializer.SpansMultipleLines())
            {
                return;
            }

            if (initializer.Expressions.SeparatorCount < initializer.Expressions.Count)
            {
                context.ReportDiagnostic(Diagnostic.Create(s_rule, initializer.Expressions.Last().GetLocation()));
            }
        }

        private static void HandleAnonymousObjectInitializer(SyntaxNodeAnalysisContext context)
        {
            var initializer = (AnonymousObjectCreationExpressionSyntax)context.Node;
            if (initializer == null || !initializer.SpansMultipleLines())
            {
                return;
            }

            if (initializer.Initializers.SeparatorCount < initializer.Initializers.Count)
            {
                context.ReportDiagnostic(Diagnostic.Create(s_rule, initializer.Initializers.Last().GetLocation()));
            }
        }
    }
}