using System;
using System.Collections.Generic;
using System.Text;

namespace StrideNet.SourceGenerator
{
    internal class CodeBuilder 
    {
        private StringBuilder _builder = new StringBuilder();

        private const int TabLength = 4;
        private int _indentionLevel = 1;
        private string CurrentIndention => new(' ', _indentionLevel * TabLength);

        public CodeBuilder(){}

        public CodeBuilder(int indentionLevel) => _indentionLevel = indentionLevel;

        public void Append(string value) => _builder.Append(PreAppendIndetion(value));

        public void AppendLine() => _builder.AppendLine();

        public void AppendLine(string value) => _builder.AppendLine(PreAppendIndetion(value));

        public void AppendBlock(string value)
        {
            IncreaseIndention();
            AppendLine(value);
            DecreaseIndention();
        }

        private string PreAppendIndetion(string value)
        {
            return CurrentIndention + value.Replace("\n", "\n" + CurrentIndention);
        }

        public void IncreaseIndention()
        {
            _indentionLevel++;
        }

        public void DecreaseIndention()
        {
            if(_indentionLevel > 1)
                _indentionLevel--;
        }

        public override string ToString() => _builder.ToString();
    }
}
