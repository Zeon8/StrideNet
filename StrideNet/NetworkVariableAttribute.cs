using Riptide;
using System;

namespace StrideNet
{

    [AttributeUsage(AttributeTargets.Field)]
    public class NetworkVariableAttribute : Attribute
    {
        public MessageSendMode SendMode { get; init; } = MessageSendMode.Reliable;
    }
}
