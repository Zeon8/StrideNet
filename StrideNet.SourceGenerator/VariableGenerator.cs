﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace StrideNet.SourceGenerator
{
    [Generator]
    public class VariableGenerator : IIncrementalGenerator
    {
        private const string AttributeName = "StrideNet.NetworkVariableAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<IFieldSymbol> pipeline =
                context.SyntaxProvider
                .ForAttributeWithMetadataName(AttributeName,
                    static (node, _) => node is VariableDeclaratorSyntax,
                    static (context, _) => (IFieldSymbol)context.TargetSymbol);

            context.RegisterSourceOutput(pipeline.Collect(), (context, source) => Execute(source, context));
        }

        private void Execute(ImmutableArray<IFieldSymbol> fields, SourceProductionContext context)
        {
            var fieldGroups = fields.GroupBy(f => f.ContainingType, SymbolEqualityComparer.Default);

            foreach (var fieldGroup in fieldGroups)
            {
                CodeBuilder builder = new(2);
                CodeBuilder registerBuilder = new(3);

                var type = (INamedTypeSymbol)fieldGroup.Key;
                foreach (IFieldSymbol symbol in fieldGroup)
                {
                    GenerateProperty(symbol, builder, registerBuilder);
                }

                string code = $@"// <autogenerated />
using Riptide;
using StrideNet;

namespace {type.ContainingNamespace}
{{
    public partial class {type.Name} : {type.BaseType} 
    {{
{builder}
        protected override void RegisterVaribles()
        {{
            base.RegisterVaribles();
{registerBuilder}
        }}
    }}
}}";
                context.AddSource($"{type.Name}.g.cs", code);
            }
        }

        private void GenerateProperty(IFieldSymbol field, CodeBuilder builder, CodeBuilder registerBuilder)
        {
            var attribute = field.GetAttributes()
                .First(a => a.AttributeClass.ToString() == AttributeName);

            string propertyName = field.Name;
            if (propertyName[0] == '_')
                propertyName = propertyName.Substring(1);

            propertyName = propertyName[0].ToString().ToUpper() + propertyName.Substring(1);
            string sendMode = attribute.GetNamedArgumentValue("SendMode") ?? "MessageSendMode.Reliable";
            string rpcName = $"__Set{propertyName}Rpc";

            registerBuilder.AppendLine($"RegisterRpc({rpcName}, NetworkAuthority.ServerAuthority, {sendMode});");
            GeneratePropertyAndMethods(field, builder, attribute, propertyName, rpcName);
        }

        private void GeneratePropertyAndMethods(IFieldSymbol field, CodeBuilder builder, AttributeData attribute, 
            string propertyName, string rpcName)
        {
            builder.AppendLine(@$"private {field.Type} {propertyName}
{{
    get => {field.Name};
    set
    {{
        {field.Name} = value;
        Message message = RpcSender.CreateRpcMessage({rpcName});
        message.Add(value);
        RpcSender.SendRpcMessage(message);
    }}
}}

private void {rpcName}(Message message, NetworkScript script)
{{
    {field.Type} value = message.Get<{field.Type}>();
    On{propertyName}Changing({field.Name}, value);
    {field.Name} = value;
    On{propertyName}Changed(value);
}}

partial void On{propertyName}Changing({field.Type} oldValue, {field.Type} newValue);
partial void On{propertyName}Changed({field.Type} value);
");
        }
    }
}
