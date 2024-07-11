using Riptide;
using Stride.Engine;
using System;
using System.Collections.Generic;

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

        public bool IsServer => NetworkManager.IsServer;
        public bool IsClient => NetworkManager.IsClient;
        public bool IsHost => NetworkManager.IsHost;

        public NetworkEntity NetworkEntity { get; private set; } = default!;

        protected NetworkManager NetworkManager => NetworkEntity.NetworkManager;
        protected RpcSender RpcSender { get; private set; } = null!;

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

        public virtual void NetworkStart(){}

        /// <summary>
        /// Implemented by Source Generator. It is used to register RPCs.
        /// </summary>
        protected virtual void RegisterRpcs() { }

        /// <summary>
        /// Implemented by Source Generator. It is used to register network varibles.
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

        public override void Cancel()
        {
            NetworkManager.RpcHandler.RemoveScript(this);
        }

        protected void RegisterRpc(RpcDelegate rpc, RpcMode mode, MessageSendMode sendMode)
        {
            NetworkManager.RpcRegistry.AddRpc(rpc, new NetworkRpc(rpc, mode, sendMode));
        }
    }
}
