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
        private readonly List<NetworkScript> _scripts = new();
        private Logger? _log;
        private Logger Log
        {
            get
            {
                _log ??= GlobalLogger.GetLogger(nameof(RpcHandler));
                return _log;
            }
        }

        public int AddScriptAndGetId(NetworkScript script)
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

        private void HandleRpc(NetworkScript script, Message message)
        {
            INetworkRpc rpc = script.RpcRegistry.GetRpc(message)
                ?? throw new Exception("Cannot proccess unregistered RPC.");

            rpc.Invoke(message);
        }

        private void HandleRpcWithAuthority(NetworkScript script, ushort clientId, Message message)
        {
            INetworkRpc rpc = script.RpcRegistry.GetRpc(message)
                ?? throw new Exception("Cannot proccess unregistered RPC.");

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
            rpc.Invoke(message);
        }

        public void RpcRecieved(ushort clientId, Message message)
        {
            if(GetScript(message) is NetworkScript script)
                HandleRpcWithAuthority(script, clientId, message);
        }

        public void RpcRecieved(Message message)
        {
            NetworkScript? script = GetScript(message);
            if (script is not null)
                HandleRpc(script, message);
        }
    }
}
