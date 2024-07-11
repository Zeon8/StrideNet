using Riptide;
using Stride.Engine;
using Stride.Games;
using StrideNet;

namespace Sample
{
    public class GameStartup : SyncScript
    {
        public NetworkManager NetworkManager { get; init; }
        public NetworkSpawner Spawner { get; init; }
        public Prefab PlayerPrefab { get; init; }

        public override void Start()
        {
            NetworkManager.ClientConnectedToServer += NetworkManager_ClientConnectedToServer; ;
            NetworkManager.ServerStarted += NetworkManager_ServerStarted;
        }

        private void NetworkManager_ServerStarted(object sender, System.EventArgs e)
        {
            if(NetworkManager.IsHost)
                Spawner.SpawnEntities(PlayerPrefab, 0);
        }

        private void NetworkManager_ClientConnectedToServer(object sender, ServerConnectedEventArgs e)
        {
            Spawner.SpawnEntities(PlayerPrefab, e.Client.Id);
        }

        public override void Update()
        {
            if (Input.IsKeyDown(Stride.Input.Keys.D1))
                NetworkManager.StartHost();

            if (Input.IsKeyDown(Stride.Input.Keys.D2))
                NetworkManager.StartClient();
        }
    }
}
