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

    /// <summary>
    /// Network RPC instance. 
    /// </summary>
    public interface INetworkRpc
    {
        /// <summary>
        /// RPC call authority.
        /// </summary>
        NetworkAuthority Mode { get; }

        /// <summary>
        /// RPC send mode.
        /// </summary>
        MessageSendMode SendMode { get; }

        /// <summary>
        /// Call received RPC with message.
        /// </summary>
        /// <param name="message">Received network message.</param>
        /// <param name="script">Script instance for RPC method.</param>
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
