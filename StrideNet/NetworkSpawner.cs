using Riptide;
using Stride.Core;
using Stride.Engine;
using Stride.Engine.Events;
using System;
using System.Collections.Generic;

namespace StrideNet
{
	/// <summary>
	/// Provides functionality to spawn entities on peers.
	/// </summary>
    [ComponentCategory("Network")]
    public class NetworkSpawner : StartupScript
    {
		/// <summary>
		/// Prefabs that could be spawned on peers. 
		/// Each entity must have at least one entity with <see cref="NetworkEntity"/> component.
		/// </summary>
        public List<Prefab> SpawnablePrefabs { get; init; } = new();
        
        private readonly List<(NetworkEntity networkEntity, ushort prefabId)> _spawnedEntities = new();
        private NetworkManager _networkManager = null!;

        public override void Start()
        {
            _networkManager = Entity.Get<NetworkManager>();
            if (_networkManager is null)
                throw new Exception("NetworkManager not found. Please put NetworkSpawner together with NetworkManager in the same entity.");

            _networkManager.ClientConnectedToServer += Server_ClientConnected;
            _networkManager.ClientDisconnectedFromServer += NetworkManager_ClientDisconnectedFromServer;
            
            _networkManager.AddClientHandler((ushort)ClientNetworkMessages.SpawnEntity, OnSpawnEntity);
            _networkManager.AddClientHandler((ushort)ClientNetworkMessages.DespawnEntity, OnDespawnEntity);
        }

        #region Event Handlers
		private void Server_ClientConnected(object? sender, ServerConnectedEventArgs e)
        {
			var server = (Server)sender!;

			// Spawn other network objects on connected client
			for (int i = 0; i < _spawnedEntities.Count; i++)
			{
				(NetworkEntity networkEntity, ushort prefabId) = _spawnedEntities[i];
				Message message = CreateSpawnMessage(prefabId, networkEntity.OwnerId);
				server.Send(message, e.Client.Id);
			}
		}

		private void NetworkManager_ClientDisconnectedFromServer(object? sender, ServerDisconnectedEventArgs e)
		{
			Log.Info($"Despawning objects related to client {e.Client.Id}");

			foreach ((NetworkEntity networkEntity, ushort prefabId) in _spawnedEntities.ToArray())
			{
				if (networkEntity.OwnerId == e.Client.Id)
					DespawnEntity(networkEntity, prefabId);
			}
		}


		private void OnSpawnEntity(Message message)
        {
            ushort prefabId = message.GetUShort();
            ushort ownerId = message.GetUShort();
            InstantiatePrefab(prefabId, ownerId);
        }

        private void OnDespawnEntity(Message message)
        {
            ushort prefabId = message.GetUShort();
            DespawnEntityLocaly(prefabId);
        }
		#endregion

		private static Message CreateSpawnMessage(ushort entityId, ushort ownerId)
        {
            var message = Message.Create(MessageSendMode.Reliable, ClientNetworkMessages.SpawnEntity);
            message.Add(entityId);
            message.Add(ownerId);
            return message;
        }

		#region Spawning
		/// <summary>
		/// Spawns entity on peers by prefab. 
		/// Prefab should be present in <see cref="SpawnablePrefabs"/> collection to be spawned.
		/// </summary>
		/// <param name="prefab">Prefab that contains network entities to spawn</param>
		/// <param name="ownerId">Identificator of the peer that will have ownership over this entity.</param>
		/// <exception cref="ArgumentException">Prefab is not present in <see cref="SpawnablePrefabs"/> collection</exception>
		public List<Entity> SpawnEntities(Prefab prefab, ushort ownerId)
        {
            int prefabId = SpawnablePrefabs.IndexOf(prefab);
            if (prefabId == -1)
                throw new ArgumentException("Cannot spawn entity that is not present in SpawnablePrefabs collection.");

            _networkManager.Send(CreateSpawnMessage((ushort)prefabId, ownerId));
            return InstantiateEntities(prefab, (ushort)prefabId, ownerId);
        }

		private void InstantiatePrefab(ushort prefabId, ushort ownerId)
		{
			if (prefabId >= SpawnablePrefabs.Count)
			{
				Log.Error($"Cannot spawn entity with id {prefabId}, because it is not registered.");
				return;
			}

			if (SpawnablePrefabs[prefabId] is not Prefab prefab)
			{
				Log.Error("Cannot spawn entity, because prefab is empty. Make sure that elements of SpawnableEntities is asigned.");
				return;
			}

			Log.Info($"Spawning prefab {prefabId} with id: {ownerId}");
			InstantiateEntities(prefab, prefabId, ownerId);
		}

		private List<Entity> InstantiateEntities(Prefab prefab, ushort prefabId, ushort ownerId)
		{
			List<Entity> entities = prefab.Instantiate();
			foreach (var entity in entities)
			{
				if (entity.Get<NetworkEntity>() is NetworkEntity networkEntity)
				{
					networkEntity.Init(_networkManager, ownerId);
					networkEntity.Entity.Scene = Entity.Scene;
					_spawnedEntities.Add((networkEntity, prefabId));
				}
			}
			return entities;
		}
		#endregion

		#region Despawning
		/// <summary>
		/// Despawns <see cref="NetworkEntity"/> from server and clients.
		/// </summary>
		/// <param name="networkEntity"></param>
		public void DespawnEntity(NetworkEntity networkEntity)
		{
			ushort entityId = (ushort)_spawnedEntities
				.FindIndex(e => e.networkEntity == networkEntity);

			DespawnEntity(networkEntity, entityId);
		}

        private void DespawnEntity(NetworkEntity networkEntity, ushort entityId)
        {
            SendDespawnMessage(entityId);
            DespawnEntityLocaly(networkEntity, entityId);
        }

		private void SendDespawnMessage(ushort entityId)
		{
			var message = Message.Create(MessageSendMode.Reliable, ClientNetworkMessages.DespawnEntity);
			message.Add(entityId);
			_networkManager.Send(message);
		}

        private void DespawnEntityLocaly(ushort id)
        {
            if (id < _spawnedEntities.Count)
                DespawnEntityLocaly(_spawnedEntities[id].networkEntity, id);
            else
                Log.Error($"Cannot despawn entity {id} that is not present in collection.");
        }

        private void DespawnEntityLocaly(NetworkEntity networkEntity, ushort id)
        {
            Entity.Scene.Entities.Remove(networkEntity.Entity);
            _spawnedEntities.RemoveAt(id);
        }
		#endregion
	}
}
