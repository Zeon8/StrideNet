using System;
using System.Collections.Generic;
using System.Text;

namespace StrideNet.SourceGenerator
{
    internal class CodeBuilder 
    {
        private StringBuilder _builder = new StringBuilder();

        private const int TabLength = 4;
        private int _spacesCount = 0;

        public CodeBuilder(){}
        public CodeBuilder(int tabLevel)
        {
            _spacesCount = tabLevel * TabLength;
        }

        private string CurrentIndention => new(' ', _spacesCount);

        public void Append(string value) => _builder.Append(AddIndention(value));

        public void AppendLine(string value) => _builder.AppendLine(AddIndention(value));
        public void AppendLineWidthTab(string value)
        {
            AddTab();
            _builder.AppendLine(AddIndention(value));
            RemoveTab();
        }

        private string AddIndention(string value)
        {
            return CurrentIndention + value.Replace("\n", "\n" + CurrentIndention);
        }

        public void AppendLine() => _builder.AppendLine();

        public void AddTab()
        {
            _spacesCount += TabLength;
        }

        public void RemoveTab()
        {
            if(_spacesCount >= TabLength)
                _spacesCount -= TabLength;
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}
