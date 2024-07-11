using Riptide;
using Stride.Core.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrideNet
{
    internal class RpcHandler
    {
        private readonly RpcRegistry _rpcRegistry;
        private readonly List<NetworkScript> _scripts = new();

        private Logger? _log;
        private Logger Log => _log ??= GlobalLogger.GetLogger(nameof(RpcHandler));

        public RpcHandler(RpcRegistry rpcRegistry)
        {
            _rpcRegistry = rpcRegistry;
        }

        public int AddScript(NetworkScript script)
        {
            int id = _scripts.Count;
            if (!_scripts.Contains(script))
                _scripts.Add(script);
            return id;
        }

        private NetworkScript? GetScript(Message message)
        {
            int scriptId = message.GetInt();
            if (scriptId < _scripts.Count)
                return _scripts[scriptId];

            return null;
        }

        public void RemoveScript(NetworkScript script)
        {
            _scripts.Remove(script);
        }

        private void InvokeRpc(NetworkScript script, Message message)
        {
            if (!_rpcRegistry.TryGetRpc(message.GetInt(), out INetworkRpc? rpc))
            {
                Log.Error("Cannot proccess unregistered RPC.");
                return;
            }

            rpc.Invoke(message, script);
        }

        private void InvokeRpcWithAuthority(NetworkScript script, ushort clientId, Message message)
        {
            if(!_rpcRegistry.TryGetRpc(message.GetInt(), out INetworkRpc? rpc))
            {
                Log.Error("Cannot proccess unregistered RPC.");
                return;
            }    

            if (rpc.Mode == RpcMode.Authority && script.OwnerId != clientId)
            {
                Log.Warning($"RPC sent from client {clientId} that doesn't have authority on network object {script.OwnerId} won't be proceded.");
                return;
            }
            else if (rpc.Mode == RpcMode.ServerAuthority)
            {
                Log.Warning($"The client {clientId} attempted to invoke a RPC allowed only for the server.");
                return;
            }
            rpc.Invoke(message, script);
        }

        public void HandleRpc(ushort clientId, Message message)
        {
            if (GetScript(message) is NetworkScript script)
                InvokeRpcWithAuthority(script, clientId, message);
        }

        public void HandleRpc(Message message)
        {
            NetworkScript? script = GetScript(message);
            if (script is not null)
                InvokeRpc(script, message);
        }
    }
}
