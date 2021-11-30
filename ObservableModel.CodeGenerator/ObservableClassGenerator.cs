using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ObservableModel.CodeGenerator.Templates;

namespace ObservableModel.CodeGenerator
{
    [Generator]
    public class AutoNotifyGenerator : ISourceGenerator
    {
        private string autoNotifyCode = new AutoNotifyTemplate().TransformText();
        private string iObservableCode = new IObservableTemplate().TransformText();
        private string dependencyGraphManager = new DependencyGraphManagerTemplate().TransformText();
        private string dependsOnCode = new DependsOnTemplate().TransformText();
        private string updateCode = new UpdateTemplate().TransformText();

        public void Initialize(GeneratorInitializationContext context)
        {
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}

            // Register the attribute source
            context.RegisterForPostInitialization((i) =>
                {
                    i.AddSource("DependsOnAttribute", dependsOnCode);
                    i.AddSource("UpdateAttribute", updateCode);
                    i.AddSource("DependencyGraphManager", dependencyGraphManager);
                    i.AddSource("IObservableModel", iObservableCode);
                    i.AddSource("AutoNotifyAttribute", autoNotifyCode);
                });

            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // retrieve the populated receiver 
            if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
                return;

            // get the added attribute, and INotifyPropertyChanged
            INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName("ObservableModel.CodeGenerator.AutoNotifyAttribute");
            INamedTypeSymbol observableSymbol = context.Compilation.GetTypeByMetadataName("ObservableModel.CodeGenerator.IObservableModel");

            // group the fields by class, and generate the source
            foreach (IGrouping<INamedTypeSymbol, IFieldSymbol> group in receiver.Fields.GroupBy(f => f.ContainingType))
            {
                string classSource = ProcessClass(group.Key, group.ToList(), attributeSymbol, observableSymbol, context);
               context.AddSource($"{group.Key.Name}.generated.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        private string ProcessClass(INamedTypeSymbol classSymbol, List<IFieldSymbol> fields, ISymbol attributeSymbol, ISymbol observableSymbol, GeneratorExecutionContext context)
        {
            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                return null; //TODO: issue a diagnostic that it must be top level
            }

            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

            var className = classSymbol.Name;
            var observableDisplayString = observableSymbol.ToDisplayString();
            var classDisplayString = new ObservableClassTemplate()
            {
                NameSpace = namespaceName,
                ObservableDisplayString = observableSymbol.ToDisplayString(),
                ClassName = classSymbol.Name,
                Properties = fields.Select((f) =>
                {
                    var propertyType = f.Type;
                    var propertyName = this.ChooseName(f.Name);
                    var fieldName = f.Name;
                    return (propertyType, propertyName, fieldName);
                }).ToArray(),
            };

            return classDisplayString.TransformText();
        }
//            // begin building the generated source
//            StringBuilder source = new StringBuilder($@"
//namespace {namespaceName}
//{{
//    public partial class {classSymbol.Name} : {observableSymbol.ToDisplayString()}
//    {{
//");

//            // if the class doesn't implement INotifyPropertyChanged already, add it
//            if (!classSymbol.Interfaces.Contains(observableSymbol))
//            {
//                source.Append("public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;");
//            }

//            // create properties for each field 
//            foreach (IFieldSymbol fieldSymbol in fields)
//            {
//                ProcessField(source, fieldSymbol, attributeSymbol);
//            }

//            source.Append("} }");
//            return source.ToString();
//        }

        private void ProcessField(StringBuilder source, IFieldSymbol fieldSymbol, ISymbol attributeSymbol)
        {
            // get the name and type of the field
            string fieldName = fieldSymbol.Name;
            ITypeSymbol fieldType = fieldSymbol.Type;

            // get the AutoNotify attribute from the field, and any associated data
            AttributeData attributeData = fieldSymbol.GetAttributes().Single(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));
            TypedConstant overridenNameOpt = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "PropertyName").Value;

            string propertyName = chooseName(fieldName, overridenNameOpt);
            if (propertyName.Length == 0 || propertyName == fieldName)
            {
                //TODO: issue a diagnostic that we can't process this field
                return;
            }

            source.Append($@"
public {fieldType} {propertyName} 
{{
    get 
    {{
        return this.{fieldName};
    }}

    set
    {{
        this.{fieldName} = value;
        this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof({propertyName})));
    }}
}}

");

            string chooseName(string fieldName, TypedConstant overridenNameOpt)
            {
                if (!overridenNameOpt.IsNull)
                {
                    return overridenNameOpt.Value.ToString();
                }

                fieldName = fieldName.TrimStart('_');
                if (fieldName.Length == 0)
                    return string.Empty;

                if (fieldName.Length == 1)
                    return fieldName.ToUpper();

                return fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1);
            }

        }

        private string ChooseName(string fieldName)
        {
            fieldName = fieldName.TrimStart('_');
            if (fieldName.Length == 0)
                return string.Empty;

            if (fieldName.Length == 1)
                return fieldName.ToUpper();

            return fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1);
        }

        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        class SyntaxReceiver : ISyntaxContextReceiver
        {
            public List<IFieldSymbol> Fields { get; } = new List<IFieldSymbol>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                // any field with at least one attribute is a candidate for property generation
                if (context.Node is FieldDeclarationSyntax fieldDeclarationSyntax
                    && fieldDeclarationSyntax.AttributeLists.Count > 0)
                {
                    foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables)
                    {
                        // Get the symbol being declared by the field, and keep it if its annotated
                        IFieldSymbol fieldSymbol = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
                        if (fieldSymbol.GetAttributes().Any(ad => ad.AttributeClass.ToDisplayString() == "ObservableModel.CodeGenerator.AutoNotifyAttribute"))
                        {
                            Fields.Add(fieldSymbol);
                        }
                    }
                }
            }
        }
    }
}