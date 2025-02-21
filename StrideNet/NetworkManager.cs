using System;
using Stride.Engine;
using Riptide;
using Riptide.Utils;
using System.Collections.Generic;

namespace StrideNet
{
    /// <summary>
    /// Handles network connection and synchronization.
    /// </summary>
    [ComponentCategory("Network")]
    public class NetworkManager : SyncScript
    {
        /// <summary>
        /// Gets server adress..
        /// </summary>
        public string Adress { get; set; } = "127.0.0.1";

        /// <summary>
        /// Gets or sets server port.
        /// </summary>
        public ushort Port { get; set; } = 3000;

        /// <summary>
        /// Gets or sets maximum player count.
        /// </summary>
        public ushort MaxPlayersCount { get; init; }

        /// <summary>
        /// Gets whether NetworkManager currently is host.
        /// </summary>
        public bool IsHost { get; private set; }

        /// <summary>
        /// Gets whether NetworkManager is server.
        /// </summary>
        public bool IsServer => _peer is Server;

        /// <summary>
        /// Gets whether NetworkManager is client.
        /// </summary>
        public bool IsClient => _peer is Client;

        internal ushort NetworkId { get; private set; } = 0;

        internal RpcHandler RpcHandler { get; private set; } = default!;
        internal RpcRegistry RpcRegistry { get; } = new();

        /// <summary>
        /// Called when server successfuly started.
        /// </summary>
        public event EventHandler? ServerStarted;

        /// <summary>
        /// Called when server stopped.
        /// </summary>
        public event EventHandler? ServerStopped;

        /// <summary>
        /// Called when client successfully connected.
        /// </summary>
        public event EventHandler<ClientConnectedEventArgs>? Connected;

        /// <summary>
        /// Called when client connected to the server.
        /// </summary>
        public event EventHandler<ServerConnectedEventArgs>? ClientConnectedToServer;

        /// <summary>
        /// Called when client disconnected from server.
        /// </summary>
        public event EventHandler<ServerDisconnectedEventArgs>? ClientDisconnectedFromServer;

        private Peer? _peer;

        public const ushort HostId = 0;

        private Dictionary<ushort, Server.MessageHandler> _serverMessageHandlers = new();
        private Dictionary<ushort, Client.MessageHandler> _clientMessageHandlers = new();

        /// <inheritdoc/>
        public override void Start()
        {
            RiptideLogger.Initialize(str => Log.Debug(str), str => Log.Info(str),
                str => Log.Warning(str), str => Log.Error(str), true);

            RpcHandler = new(RpcRegistry);
            AddServerHandler((ushort)ServerNetworkMessages.Rpc, RpcHandler.HandleRpc);
            AddClientHandler((ushort)ClientNetworkMessages.Rpc, RpcHandler.HandleRpc);
        }

        /// <summary>
        /// Starts a game server on specified port.
        /// </summary>
        public void StartServer()
        {
            var server = new Server();
            _peer = server;

            server.Start(Port, MaxPlayersCount, messageHandlerGroupId: 0, useMessageHandlers: false);
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
            server.MessageReceived += Server_MessageReceived;

            NetworkId = HostId;
            ServerStarted?.Invoke(this, EventArgs.Empty);
        }

        private void Server_MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            if(_serverMessageHandlers.TryGetValue(e.MessageId, out Server.MessageHandler? handler))
            {
                handler.Invoke(e.FromConnection.Id, e.Message);
            }
        }

        /// <summary>
        /// Stops server.
        /// </summary>
        public void StopServer()
        {
            if (_peer is Server server)
            {
                server.Stop();
                IsHost = false;
            }
        }

        /// <summary>
        /// Starts a server in host mode. 
        /// </summary>
        public void StartHost()
        {
            IsHost = true;
            StartServer();
        }

        /// <summary>
        /// Intantiates client and attempts to connect to server.
        /// </summary>
        public void StartClient(Message? messsage = null)
        {
            var client = new Client();
            _peer = client;

            client.Connected += Client_Connected;
            client.Connect(Adress + ':' + Port, maxConnectionAttempts: 5, 
                messageHandlerGroupId: 0, messsage, useMessageHandlers: false);
            client.MessageReceived += Client_MessageReceived;
        }

        private void Client_MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            if (_clientMessageHandlers.TryGetValue(e.MessageId, out Client.MessageHandler? handler))
            {
                handler?.Invoke(e.Message);
            }
        }

        /// <summary>
        /// Disconnects client from server.
        /// </summary>
        public void StopClient()
        {
            if (_peer is Client client)
                client.Disconnect();
        }

        private void Server_ClientDisconnected(object? sender, ServerDisconnectedEventArgs e)
        {
            ClientDisconnectedFromServer?.Invoke(sender, e);
        }

        private void Server_ClientConnected(object? sender, ServerConnectedEventArgs e)
        {
            ClientConnectedToServer?.Invoke(sender, e);
        }

        private void Client_Connected(object? sender, EventArgs e)
        {
            var client = (Client)sender!;
            NetworkId = client.Id;
        }

        /// <inheritdoc/>
        public override void Update()
        {
            _peer?.Update();
        }

        internal void Send(Message message)
        {
            if (_peer is Server server)
            {
                server.SendToAll(message);
            }
            else if (_peer is Client client)
            {
                client.Send(message);
            }
        }

        internal void SendTo(ushort clientId, Message message)
        {
            if (_peer is Server server)
            {
                server.Send(message, clientId);
            }
        }


        internal void AddServerHandler(ushort id, Server.MessageHandler handler)
        {
            if (_serverMessageHandlers.ContainsKey(id))
                throw new ArgumentException("Cannot add handler, because handler with same id was already added.");
            
            _serverMessageHandlers.Add(id, handler);
        }

        internal void AddClientHandler(ushort id, Client.MessageHandler handler)
        {
            if (_clientMessageHandlers.ContainsKey(id))
                throw new ArgumentException("Cannot add handler, because handler with same id was already added.");
            
            _clientMessageHandlers.Add(id, handler);
        }
    }
}
