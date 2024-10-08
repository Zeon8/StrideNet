﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace StrideNet.SourceGenerator
{
    [Generator]
    public class RpcGenerator : IIncrementalGenerator
    {
        private const string AttributeName = "StrideNet.NetworkRpcAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            IncrementalValuesProvider<(MethodDeclarationSyntax, IMethodSymbol)> pipeline = context.SyntaxProvider
                .ForAttributeWithMetadataName(AttributeName,
                static (node, _) => node is MethodDeclarationSyntax,
                static (context, _) => ((MethodDeclarationSyntax)context.TargetNode, (IMethodSymbol)context.TargetSymbol));

            context.RegisterSourceOutput(pipeline.Collect(), (spc, source) => Execute(source, spc));
        }

        private void Execute(ImmutableArray<(MethodDeclarationSyntax, IMethodSymbol Symbol)> methodSyntaxes, SourceProductionContext context)
        {
            var methodGroups = methodSyntaxes
                .GroupBy(m => m.Symbol.ContainingType, SymbolEqualityComparer.Default);

            foreach (var methodGroup in methodGroups)
            {
                CodeBuilder registersBuilder = new(3);
                CodeBuilder wrappersBuilder = new(2);
                var type = (INamedTypeSymbol)methodGroup.Key;

                foreach ((MethodDeclarationSyntax syntax, IMethodSymbol symbol) in methodGroup)
                {
                    AttributeData attribute = symbol.GetAttributes()
                        .First(a => a.AttributeClass.ToString().Contains(AttributeName));

                    string methodName = GetCallMethodName(symbol.Name);
                    GenerateRpcRegistration(methodName, attribute, registersBuilder);
                    GenerateCallWrapper(symbol, wrappersBuilder);
                    GenerateSendWrapper(syntax, symbol, attribute, wrappersBuilder);
                }

                string code = $@"// <autogenerated />
using Riptide;
using StrideNet;
using System.ComponentModel;

namespace {type.ContainingNamespace}
{{
    public partial class {type.Name} : {type.BaseType} 
    {{
{wrappersBuilder}

        protected override void RegisterRpcs()
        {{
            base.RegisterRpcs();{registersBuilder}
        }}
    }}
}}";
                context.AddSource($"{type.Name}.g.cs", code);
            }

        }

        private static void GenerateRpcRegistration(string methodName, AttributeData attribute, CodeBuilder builder)
        {
            string rpcAuthority = attribute.GetNamedArgumentValue("Authority") ?? "NetworkAuthority.OwnerAuthority";
            string sendMode = attribute.GetNamedArgumentValue("SendMode") ?? "MessageSendMode.Reliable";
            builder.AppendLine();
            builder.Append(@$"RegisterRpc({methodName}, {rpcAuthority}, {sendMode});");
        }

        private static void GenerateCallWrapper(IMethodSymbol symbol, CodeBuilder builder)
        {
            string methodName = symbol.Name;
            string callMethodName = GetCallMethodName(methodName);
            builder.AppendLine($"private static void {callMethodName}(Message message, NetworkScript script)");
            builder.AppendLine("{");
            builder.IncreaseIndention();
            foreach (var param in symbol.Parameters)
                builder.AppendLine($"{param.Type} {param.Name} = message.Get<{param.Type}>();");
            
            string argumentList = string.Join(",", symbol.Parameters.Select(p => p.Name));
            builder.AppendLine($"(({symbol.ContainingType.Name})script).{methodName}({argumentList});");
            builder.DecreaseIndention();
            builder.AppendLine("}");
            builder.AppendLine();

        }

        private static void GenerateSendWrapper(MethodDeclarationSyntax syntax, IMethodSymbol symbol,
            AttributeData attribute, CodeBuilder builder)
		{
			string rpcMethodName = symbol.Name + "Rpc";
			string methodParameters = string.Join(", ", 
                symbol.Parameters.Select(p => $"{p.Type} {p.Name}"));

            var rpcAuthority = attribute.GetNamedArgumentValue("Authority")
                ?? "StrideNet.NetworkAuthority.OwnerAuthority";

            string arguments = string.Join(", ", symbol.Parameters.Select(p => p.Name));
            string originalMethodCall = $"{symbol.Name}({arguments});";

            var callMethodName = GetCallMethodName(symbol.Name);

			builder.AppendLine($"{syntax.Modifiers} {symbol.ReturnType} {rpcMethodName}({methodParameters})");
			builder.AppendLine("{");
			builder.IncreaseIndention();

            if (rpcAuthority == "StrideNet.NetworkAuthority.ServerAuthority")
            {
                builder.AppendLine(originalMethodCall);
                builder.AppendLine(@"if (IsClient)");
                builder.AppendBlock(@"RpcExceptions.ThrowCalledServerAuthorative();");
            }
            else
            {
                builder.AppendLine(@$"if (IsServer)
{{
    {originalMethodCall}
    if(IsOwner)
        return;
}}");

                if (rpcAuthority == "StrideNet.NetworkAuthority.OwnerAuthority")
                {
                    builder.AppendLine("else if (!IsOwner)");
                    builder.AppendBlock("RpcExceptions.ThrowCalledForeignEntity();");
                    
                }
                builder.AppendLine();
            }

            builder.AppendLine($"Message message = RpcSender.CreateRpcMessage({callMethodName});");
            foreach (var param in symbol.Parameters)
                builder.AppendLine($"message.Add({param.Name});");
			builder.AppendLine($"RpcSender.SendRpcMessage(message);");
			builder.DecreaseIndention();
			builder.AppendLine("}");


        }

		private static string GetCallMethodName(string methodName)
        {
            return $"__Call{methodName}Rpc";
        }
    }
}
