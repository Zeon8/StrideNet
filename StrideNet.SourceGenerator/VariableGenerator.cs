﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
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

            string propertyName = field.Name.Replace("_", "");
            propertyName = propertyName[0].ToString().ToUpper() + propertyName.Substring(1);
            string sendMode = attribute.GetNamedArgumentValue("SendMode") ?? "MessageSendMode.Reliable";
            string rpcName = $"Set{propertyName}Rpc";

            registerBuilder.AppendLine($"RegisterRpc({rpcName}, RpcMode.ServerAuthority, {sendMode});");
            GenerateProperty(field, builder, attribute, propertyName, rpcName);
        }

        private void GenerateProperty(IFieldSymbol field, CodeBuilder builder, AttributeData attribute, 
            string propertyName, string rpcName)
        {
            string accessModifier = FromEnum(attribute.GetNamedArgumentValue("Modifier"));

            string notificationMethod = attribute.NamedArguments
                .FirstOrDefault(a => a.Key == "NotificationMethod").Value.Value as string;
           
            builder.AppendLine(@$"{accessModifier} {field.Type} {propertyName}
{{
    get => {field.Name};
    set
    {{
        {field.Name} = value;
        RpcSender.CreateRpc({rpcName}, out INetworkRpc rpc, out Message message);
        message.Add(value);
        RpcSender.SendRpc(rpc, message);
    }}
}}

private void {rpcName}(Message message)
{{
    {field.Type} value = message.Get<{field.Type}>();");

            builder.AddTab();
            if (!string.IsNullOrEmpty(notificationMethod))
                builder.AppendLine($"{notificationMethod}({field.Name}, value);");

            builder.AppendLine($"{field.Name} = value;");
            builder.RemoveTab();
            builder.AppendLine("}");
        }

        private string FromEnum(string modifier)
        {
            return modifier switch
            {
                "StrideNet.AccessModifier.Private" => "private",
                "StrideNet.AccessModifier.Public" => "public",
                "StrideNet.AccessModifier.Protected" => "protected",
                "StrideNet.AccessModifier.Internal" => "internal",
                _ => "private",
            };
        }
    }
}
