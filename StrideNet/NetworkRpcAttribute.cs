using Riptide;
using System;
using System.Runtime.InteropServices;

namespace StrideNet
{
    /// <summary>
    /// Indicates that attributed method is RPC. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NetworkRpcAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets RPC call authority.
        /// </summary>
        public NetworkAuthority Authority { get; set; } = NetworkAuthority.OwnerAuthority;

        /// <summary>
        /// Gets or sets send mode of the RPC.
        /// </summary>
        public MessageSendMode SendMode { get; set; } = MessageSendMode.Unreliable;
    }
}
