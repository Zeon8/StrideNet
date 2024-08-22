using Riptide;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace StrideNet
{
    /// <summary>
    /// Script that allows to call RPCs and syncronize variables over network.
    /// </summary>
    public abstract class NetworkScript : SyncScript
    {
        /// <inheritdoc cref="NetworkEntity.IsOwner"/>
        public bool IsOwner => NetworkEntity.IsOwner;

        /// <inheritdoc cref="NetworkEntity.HasAuthority"/>
        public bool HasAuthority => NetworkEntity.HasAuthority;

        /// <inheritdoc cref="NetworkEntity.OwnerId"/>
        public ushort OwnerId => NetworkEntity.OwnerId;

        /// <inheritdoc cref="NetworkManager.IsServer"/>
        public bool IsServer => NetworkManager.IsServer;

        /// <inheritdoc cref="NetworkManager.IsServer"/>
        public bool IsClient => NetworkManager.IsClient;

        /// <inheritdoc cref="NetworkManager.IsServer"/>
        public bool IsHost => NetworkManager.IsHost;

        /// <summary>
        /// Gets <see cref="StrideNet.NetworkEntity"/> component of this entity.
        /// </summary>
        public NetworkEntity NetworkEntity { get; private set; } = default!;

        /// <summary>
        /// Gets <see cref="StrideNet.NetworkManager"/> instance.
        /// </summary>
        protected NetworkManager NetworkManager => NetworkEntity.NetworkManager;

        /// <summary>
        /// Gets a RpcSender.
        /// </summary>
        protected RpcSender RpcSender { get; private set; } = null!;

        /// <inheritdoc/>
        public override sealed void Start()
        {
            NetworkEntity = GetNetworkEntity()!;
            if (NetworkEntity is null)
                throw new ArgumentException("NetworkEntity component is not found in this entity and parent entities. Make sure that you added NetworkObject to the entity along with script.");
            
            var scriptId = (ushort)NetworkManager.RpcHandler.AddScript(this); 

            RpcSender = new(NetworkManager.RpcRegistry, NetworkEntity.NetworkManager, scriptId);
            RegisterRpcs();
            RegisterVaribles();

            NetworkStart();
        }

        /// <summary>
        /// Called after script initialization.
        /// </summary>
        public virtual void NetworkStart(){}

        /// <summary>
        /// Implemented by Source Generator. Used for registering RPCs.
        /// </summary>
        protected virtual void RegisterRpcs() { }

        /// <summary>
        /// Implemented by Source Generator. Used for registering network varibles.
        /// </summary>
        protected virtual void RegisterVaribles() { }

        private NetworkEntity? GetNetworkEntity()
        {
            Entity entity = Entity;
            NetworkEntity? networkEntity = null;
            while (entity is not null && networkEntity is null)
            {
                networkEntity = entity.Get<NetworkEntity>();
                entity = entity.GetParent();
            }
            return networkEntity;
        }

        /// <inheritdoc/>
        public override void Cancel()
        {
            NetworkManager.RpcHandler.RemoveScript(this);
        }

        /// <summary>
        /// Adds RPC to registry.
        /// </summary>
        /// <param name="rpc">RPC method</param>
        /// <param name="authority">RPC call authority</param>
        /// <param name="sendMode">Send mode</param>
        protected void RegisterRpc(RpcDelegate rpc, NetworkAuthority authority, MessageSendMode sendMode)
        {
            NetworkManager.RpcRegistry.AddRpc(rpc, new NetworkRpc(rpc, authority, sendMode));
        }
    }
}
