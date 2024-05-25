using Riptide;
using System.Collections.Generic;

namespace StrideNet
{
    internal class RpcRegistry
    {
        private readonly List<INetworkRpc> _rpcs = new();
        private readonly Dictionary<RpcDelegate, int> _rpcsCache = new();

        public void AddRpc(RpcDelegate rpcDelegate, INetworkRpc rpc)
        {
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

        public bool TryGetRpc(RpcDelegate rpcDelegate, out INetworkRpc? rpc, out int rpcId)
        {
            rpc = GetRpc(rpcDelegate, out rpcId);
            return rpc is not null;
        }

        public INetworkRpc? GetRpc(Message message)
        {
            int rpcId = message.GetInt();
            if (rpcId < _rpcs.Count)
                return _rpcs[rpcId];
            return null;
        }
    }
}
