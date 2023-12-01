using Riptide;
using Stride.Engine;
using System;
using System.Collections.Generic;

namespace StrideNet
{
    /// <summary>
    /// Script that allows to call RPCs and syncronize varibles over network.
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

        protected NetworkManager NetworkManager => NetworkEntity.NetworkManager;
        internal RpcRegistry RpcRegistry { get; } = new();
        protected RpcSender RpcSender { get; private set; } = null!;
        private RpcHandler RpcHandler => NetworkManager.RpcHandler;

        private NetworkEntity _networkEntity = null!;
        private NetworkEntity NetworkEntity
        {
            get
            {
                if (_networkEntity is null)
                    throw new InvalidOperationException("NetworkScript is not initialized. " +
                        "Make sure you called base method in your Start method.");
                return _networkEntity;
            }
        }

        public override void Start()
        {
            _networkEntity = GetNetworkEntity()!;
            if (_networkEntity is null)
                throw new ArgumentException("NetworkEntity component is not found in this entity and parent entities. Make sure that you added NetworkObject to the entity along with script.");
            
            // Save index of added script
            var scriptId = (ushort)RpcHandler.AddScriptAndGetId(this); 

            RpcSender = new(RpcRegistry, _networkEntity.NetworkManager, scriptId);
            RegisterRpcs();
            RegisterVaribles();
        }

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
            RpcHandler.RemoveScript(this);
        }

        protected void RegisterRpc(RpcDelegate rpc, RpcMode mode, MessageSendMode sendMode)
        {
            RpcRegistry.AddRpc(rpc, new NetworkRpc(rpc, mode, sendMode));
        }
    }
}
