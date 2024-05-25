using Stride.Core;
using Stride.Engine;

namespace StrideNet
{
    [ComponentCategory("Network")]
    public class NetworkEntity : StartupScript
    {
        /// <summary>
        /// Network identifier for network entity. Determines ownership over entity.
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
