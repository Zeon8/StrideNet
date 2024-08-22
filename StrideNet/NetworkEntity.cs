using Stride.Core;
using Stride.Engine;

namespace StrideNet
{
    /// <summary>
    /// Representation of entity in network.
    /// </summary>
    [ComponentCategory("Network")]
    public class NetworkEntity : StartupScript
    {
        /// <summary>
        /// Gets the network entity owner identifier. 
        /// </summary>
        [DataMemberIgnore]
        public ushort OwnerId { get; private set; }

        [DataMemberIgnore]
        internal NetworkManager NetworkManager { get; private set; } = null!;

        internal void Init(NetworkManager manager, ushort id)
        {
            OwnerId = id;
            NetworkManager = manager;
        }

        /// <summary>
        /// Whether or not the peer owns this entity.
        /// </summary>
        public bool IsOwner => OwnerId == NetworkManager.NetworkId;

        /// <summary>
        /// Whether or not the peer has authority over this entity.
        /// Server has authority over all entities.
        /// </summary>
        public bool HasAuthority => IsOwner || NetworkManager.IsServer;
    }
}
