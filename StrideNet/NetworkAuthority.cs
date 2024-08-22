using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrideNet
{
    /// <summary>
    /// RPC call authority.
    /// </summary>
    public enum NetworkAuthority
    {
        /// <summary>
        /// Any peer can call the PRC.
        /// </summary>
        NoAuthority,

        /// <summary>
        /// Only peer that owns entity can call the RPC on the server.
        /// </summary>
        OwnerAuthority,

        /// <summary>
        /// Only server can call the RPC on the peers.
        /// </summary>
        ServerAuthority
    }
}
