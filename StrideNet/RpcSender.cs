using Riptide;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StrideNet
{
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
     
        private Message CreateMessage(int rpcId, MessageSendMode sendMode)
        {
            Enum rpcMode = _networkManager.IsClient
                                ? ClientNetworkMessages.Rpc
                                : ServerNetworkMessages.Rpc;

            var message = Message.Create(sendMode, rpcMode);
            message.Add(_scriptId);
            message.Add(rpcId);
            return message;
        }

        public void CreateRpc(RpcDelegate rpcDelegate, out INetworkRpc? rpc, out Message message)
        {
            if (!_registry.TryGetRpc(rpcDelegate, out rpc, out int rpcId))
                throw new ArgumentException("Cannot create unregistered RPC.");

            message = CreateMessage(rpcId, rpc.SendMode);
        }

        public void SendRpc(INetworkRpc rpc, Message message)
        {
            _networkManager.Send(message);
        }
    }
}
