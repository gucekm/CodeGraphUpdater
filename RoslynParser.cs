using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class RoslynParser
{
    public List<ClassInfo> ParseClasses(SyntaxNode root, string filePath)
    {
        var result = new List<ClassInfo>();
        var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

        foreach (var cls in classes)
        {
            var info = new ClassInfo
            {
                Name = cls.Identifier.Text,
                Summary = GetSummary(cls),
                FilePath = filePath,
                BaseType = cls.BaseList?.Types.FirstOrDefault()?.ToString(),
                SourceCode = cls.ToFullString(), // <-- Add this
                Methods = ParseMethods(cls),
                Properties = ParseProperties(cls),
                Events = ParseEvents(cls),
                Fields = ParseFields(cls),
                Constructors = ParseConstructors(cls)
                
            };
            result.Add(info);
        }

        return result;
    }

    public List<string> ParseInterfaces(SyntaxNode root)
    {
        return root.DescendantNodes()
                   .OfType<InterfaceDeclarationSyntax>()
                   .Select(i => i.Identifier.Text)
                   .ToList();
    }

    private List<MethodInfo> ParseMethods(ClassDeclarationSyntax cls)
    {
        var methods = new List<MethodInfo>();
        var methodNodes = cls.DescendantNodes().OfType<MethodDeclarationSyntax>();

        foreach (var method in methodNodes)
        {
            var methodInfo = new MethodInfo
            {
                Name = method.Identifier.Text,
                Summary = GetSummary(method),
                Parameters = string.Join(", ", method.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier.Text}")),
                ReturnType = method.ReturnType.ToString(),
                SourceCode = method.ToFullString(), // <-- Add this
                Calls = method.DescendantNodes()
                              .OfType<InvocationExpressionSyntax>()
                              .Select(call => call.Expression.ToString().Split('.').Last())
                              .ToList(),
                SubscribesToEvents = method.DescendantNodes()
                    .OfType<AssignmentExpressionSyntax>()
                    .Where(a => a.Kind() == SyntaxKind.AddAssignmentExpression)
                    .Select(a => a.Left.ToString())
                    .ToList(),
                RaisesEvents = method.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(i => i.Expression.ToString().EndsWith(".Invoke") || i.Expression is IdentifierNameSyntax)
                    .Select(i => i.Expression.ToString().Replace(".Invoke", ""))
                    .ToList()
            };

            methods.Add(methodInfo);
        }

        return methods;
    }

    private List<PropertyInfo> ParseProperties(ClassDeclarationSyntax cls)
    {
        return cls.DescendantNodes().OfType<PropertyDeclarationSyntax>().Select(prop => new PropertyInfo
        {
            Name = prop.Identifier.Text,
            Type = prop.Type.ToString(),
            Summary = GetSummary(prop),
            Modifiers = prop.Modifiers.Select(m => m.Text).ToList(),
            SourceCode = prop.ToFullString() // <-- Add this
        }).ToList();
    }

    private List<EventInfo> ParseEvents(ClassDeclarationSyntax cls)
    {
        return cls.DescendantNodes().OfType<EventDeclarationSyntax>().Select(evt => new EventInfo
        {
            Name = evt.Identifier.Text,
            Type = evt.Type.ToString(),
            Summary = GetSummary(evt),
            SourceCode = evt.ToFullString() // <-- Add this
        }).ToList();
    }

    private List<FieldInfo> ParseFields(ClassDeclarationSyntax cls)
    {
        return cls.DescendantNodes().OfType<FieldDeclarationSyntax>().SelectMany(field =>
            field.Declaration.Variables.Select(v => new FieldInfo
            {
                Name = v.Identifier.Text,
                Type = field.Declaration.Type.ToString(),
                Modifiers = field.Modifiers.Select(m => m.Text).ToList(),
                Summary = GetSummary(field),
                SourceCode = field.ToFullString() // <-- Add this
            })
        ).ToList();
    }

    private List<ConstructorInfo> ParseConstructors(ClassDeclarationSyntax cls)
    {
        return cls.DescendantNodes().OfType<ConstructorDeclarationSyntax>().Select(ctor => new ConstructorInfo
        {
            Name = ctor.Identifier.Text,
            Summary = GetSummary(ctor),
            SourceCode = ctor.ToFullString(), // <-- Add this
            Parameters = string.Join(", ", ctor.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier.Text}")),
            Modifiers = ctor.Modifiers.Select(m => m.Text).ToList()
        }).ToList();
    }

    private string GetSummary(SyntaxNode node)
    {
        var trivia = node.GetLeadingTrivia()
                         .Select(i => i.GetStructure())
                         .OfType<DocumentationCommentTriviaSyntax>()
                         .FirstOrDefault();

        var summary = trivia?.ChildNodes()
                             .OfType<XmlElementSyntax>()
                             .FirstOrDefault(e => e.StartTag.Name.LocalName.Text == "summary");

        return summary?.Content.ToString().Trim().Replace("\n", " ").Replace("\r", " ");
    }
}
