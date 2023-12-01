using Riptide;
using System;

namespace StrideNet
{
    public enum AccessModifier
    {
        Private,
        Protected,
        Internal,
        Public,
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class NetworkVariableAttribute : Attribute
    {
        public AccessModifier Modifier { get; init; } = AccessModifier.Private;

        public MessageSendMode SendMode { get; init; } = MessageSendMode.Reliable;

        public string? NotificationMethod { get; init; }
    }
}
