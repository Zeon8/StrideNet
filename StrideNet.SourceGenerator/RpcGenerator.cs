﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
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

                foreach (var (syntax, symbol) in methodGroup)
                {
                    AttributeData attribute = symbol.GetAttributes()
                        .First(a => a.AttributeClass.ToString().Contains(AttributeName));
                    GenerateRpcRegistration(symbol, attribute, registersBuilder);
                    GenerateCallWrapper(symbol, wrappersBuilder);
                    GenerateSendWrapper(syntax, symbol, attribute, wrappersBuilder);
                }

                string code = $@"// <autogenerated />
using Riptide;
using StrideNet;
namespace {type.ContainingNamespace}
{{
    public partial class {type.Name} : {type.BaseType} 
    {{
        protected override void RegisterRpcs()
        {{
            base.RegisterRpcs();
{registersBuilder}
        }}

{wrappersBuilder}
    }}
}}";
                context.AddSource($"{type.Name}.g.cs", code);
            }

        }

        private static void GenerateRpcRegistration(IMethodSymbol methodSymbol, AttributeData attribute, CodeBuilder builder)
        {
            string rpcMode = attribute.GetNamedArgumentValue("Mode") ?? "RpcMode.Authority";
            string sendMode = attribute.GetNamedArgumentValue("SendMode") ?? "MessageSendMode.Reliable";

            string methodName = GetCallMethodName(methodSymbol.Name);
            builder.AppendLine(@$"RegisterRpc({methodName}, {rpcMode}, {sendMode});");
        }

        private static void GenerateCallWrapper(IMethodSymbol symbol, CodeBuilder builder)
        {
            string methodName = symbol.Name;
            string callMethodName = GetCallMethodName(methodName);
            builder.AppendLine($"private static void {callMethodName}(Message message, NetworkScript script)");
            builder.AppendLine("{");
            builder.AddTab();
            foreach (var param in symbol.Parameters)
                builder.AppendLine($"{param.Type} {param.Name} = message.Get<{param.Type}>();");
            
            string argumentList = string.Join(",", symbol.Parameters.Select(p => p.Name));
            builder.AppendLine($"(({symbol.ContainingType.Name})script).{methodName}({argumentList});");
            builder.RemoveTab();
            builder.AppendLine("}");
            builder.AppendLine();

        }

        private static void GenerateSendWrapper(MethodDeclarationSyntax syntax, IMethodSymbol symbol,
            AttributeData attribute, CodeBuilder builder)
		{
			string paramsList = string.Join(", ", symbol.Parameters.Select(p => $"{p.Type} {p.Name}"));
			string rpcMethodName = symbol.Name + "Rpc";
			var callMethodName = GetCallMethodName(symbol.Name);

			builder.AppendLine($"{syntax.Modifiers} {symbol.ReturnType} {rpcMethodName}({paramsList})");
			builder.AppendLine("{");
			builder.AddTab();

			var rpcMode = attribute.GetNamedArgumentValue("Mode");
			if (rpcMode == "StrideNet.RpcMode.ServerAuthority")
			{
				builder.AppendLine("if(IsClient)");
				builder.AppendLineWidthTab("return;");
			}

			void GenerateMethodCall()
			{
				string argumentList = string.Join(",", symbol.Parameters.Select(p => p.Name));
				builder.AppendLine($"{symbol.Name}({argumentList});");
			}

			if (rpcMode is null || rpcMode == "StrideNet.RpcMode.Authority")
			{
				builder.AppendLine(@"if(IsServer)");
				builder.AppendLine("{");
				builder.AddTab();
				GenerateMethodCall();
				builder.AppendLine("return;");
				builder.RemoveTab();
				builder.AppendLine("}");
			}
            else
			    GenerateMethodCall();

			builder.AppendLine($"RpcSender.CreateRpc({callMethodName}, out INetworkRpc rpc, out Message message);");
			if (symbol.Parameters.Any())
			{
				foreach (var param in symbol.Parameters)
					builder.AppendLine($"message.Add({param.Name});");
			}
			builder.AppendLine($"RpcSender.SendRpc(rpc, message);");
			builder.RemoveTab();
			builder.AppendLine("}");

		}

		private static string GetCallMethodName(string methodName)
        {
            return $"Call{methodName}Rpc";
        }
    }
}
