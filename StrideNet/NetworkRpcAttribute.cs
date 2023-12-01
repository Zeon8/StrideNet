using Riptide;
using System;

namespace StrideNet
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NetworkRpcAttribute : Attribute
    {
        public RpcMode Mode { get; set; } = RpcMode.Authority;

        public MessageSendMode SendMode { get; set; } = MessageSendMode.Unreliable;
    }
}
