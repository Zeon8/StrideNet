using Riptide;
using System;
using System.ComponentModel;

namespace StrideNet
{
    /// <summary>
    /// Allows creating and sending messages to server.
    /// </summary>
    public class RpcSender
    {
        private readonly RpcRegistry _registry;
        private readonly NetworkManager _networkManager;
        private readonly int _scriptId;

        internal RpcSender(RpcRegistry registry, NetworkManager manager, int scriptId)
        {
            _registry = registry;
            _networkManager = manager;
            _scriptId = scriptId;
        }
     
        private Message CreateRpcMessage(int rpcId, MessageSendMode sendMode)
        {
            Enum rpcMode = _networkManager.IsClient
                                ? ClientNetworkMessages.Rpc
                                : ServerNetworkMessages.Rpc;

            var message = Message.Create(sendMode, rpcMode);
            message.Add(_scriptId);
            message.Add(rpcId);
            return message;
        }

        /// <summary>
        /// Creates a RPC message.
        /// </summary>
        /// <param name="rpcDelegate">RPC method</param>
        /// <returns>Created message.</returns>
        /// <exception cref="ArgumentException">Rpc is not registered in <see cref="RpcRegistry"/></exception>
        public Message CreateRpcMessage(RpcDelegate rpcDelegate)
        {
            if (!_registry.TryGetRpc(rpcDelegate, out INetworkRpc? rpc, out int rpcId))
                throw new ArgumentException("Cannot create message for unregistered RPC.");

            return CreateRpcMessage(rpcId, rpc.SendMode);
        }

        /// <summary>
        /// Sends RPC message over network.
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendRpcMessage(Message message)
        {
            _networkManager.Send(message);
        }
    }
}
