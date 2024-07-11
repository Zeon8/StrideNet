using Riptide;
using System;

namespace StrideNet
{
    public delegate void RpcDelegate(Message message, NetworkScript script);

    public interface INetworkRpc
    {
        RpcMode Mode { get; }
        MessageSendMode SendMode { get; }
        void Invoke(Message message, NetworkScript script);
    }

    internal class NetworkRpc : INetworkRpc
    {
        public RpcMode Mode { get; }

        public MessageSendMode SendMode { get; }

        private readonly RpcDelegate _rpc;

        public NetworkRpc(RpcDelegate rpc, RpcMode mode, MessageSendMode sendMode)
        {
            _rpc = rpc;
            Mode = mode;
            SendMode = sendMode;
        }

        public void Invoke(Message message, NetworkScript script) => _rpc(message, script);
    }
}
