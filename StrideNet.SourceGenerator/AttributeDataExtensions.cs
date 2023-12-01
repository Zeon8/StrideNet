using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace StrideNet.SourceGenerator
{
    internal static class AttributeDataExtensions
    {
        public static string GetNamedArgumentValue(this AttributeData attribute, string name)
        {
            var item = attribute.NamedArguments.FirstOrDefault(a => a.Key == name);
            if (item.Value.IsNull)
                return null;
            return item.Value.ToCSharpString();
        }
    }
}
