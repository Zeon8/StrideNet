using Riptide;
using System;

namespace StrideNet
{
    /// <summary>
    /// Delegate for calling coresponding RPC method.
    /// </summary>
    /// <param name="message">Received message.</param>
    /// <param name="script">Network script of RPC method.</param>
    public delegate void RpcDelegate(Message message, NetworkScript script);

    internal interface INetworkRpc
    {
        NetworkAuthority Mode { get; }
        MessageSendMode SendMode { get; }
        void Call(Message message, NetworkScript script);
    }

    internal class NetworkRpc : INetworkRpc
    {
        public NetworkAuthority Mode { get; }

        public MessageSendMode SendMode { get; }

        private readonly RpcDelegate _rpc;

        public NetworkRpc(RpcDelegate rpc, NetworkAuthority mode, MessageSendMode sendMode)
        {
            _rpc = rpc;
            Mode = mode;
            SendMode = sendMode;
        }

        public void Call(Message message, NetworkScript script) => _rpc(message, script);
    }
}
