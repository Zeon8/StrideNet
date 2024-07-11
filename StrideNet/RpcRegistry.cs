using Riptide;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.AccessControl;

namespace StrideNet
{
    internal class RpcRegistry
    {
        private readonly List<INetworkRpc> _rpcs = new();
        private readonly Dictionary<RpcDelegate, int> _rpcsCache = new();

        public void AddRpc(RpcDelegate rpcDelegate, INetworkRpc rpc)
        {
            if (_rpcsCache.ContainsKey(rpcDelegate))
                return;

            int id = _rpcs.Count;
            _rpcs.Add(rpc);
            _rpcsCache.Add(rpcDelegate, id);
        }
        
        public INetworkRpc? GetRpc(RpcDelegate rpcDelegate, out int rpcId)
        {
            if (_rpcsCache.TryGetValue(rpcDelegate, out rpcId))
                return _rpcs[rpcId];
            return null;
        }

        public bool TryGetRpc(RpcDelegate rpcDelegate, [NotNullWhen(true)] out INetworkRpc? rpc, out int rpcId)
        {
            rpc = GetRpc(rpcDelegate, out rpcId);
            return rpc is not null;
        }

        public INetworkRpc? GetRpc(int id)
        {
            int rpcId = id;
            if (rpcId < _rpcs.Count)
                return _rpcs[rpcId];
            return null;
        }

        public bool TryGetRpc(int id, [NotNullWhen(true)] out INetworkRpc? rpc)
        {
            rpc = GetRpc(id);
            return rpc is not null;
        }
    }
}
