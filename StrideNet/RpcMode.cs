using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrideNet
{
    /// <summary>
    /// Call mode of RPC method.
    /// </summary>
    public enum RpcMode
    {
        /// <summary>
        /// Any peer can call the PRC.
        /// </summary>
        AnyPeer,

        /// <summary>
        /// Only peer that owns entity can call the RPC on the server.
        /// </summary>
        Authority,

        /// <summary>
        /// Only server can call the RPC on the peers.
        /// </summary>
        ServerAuthority
    }
}
