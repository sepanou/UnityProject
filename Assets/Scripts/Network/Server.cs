using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Network.Packet;

namespace Network {
	public class Server: IDisposable {
		protected class ServerClient: Client {
			public Server Server { get; }
			public ServerClient(Server server, TcpClient tcpClient) : base(tcpClient) { Server = server; }
			protected override void OnDisconnect() => Server.DisconnectClient(this);
			protected override void OnReceive() => Server.OnReceive(this);
		}

		private bool _disposed = false;
		private readonly TcpListener _tcpListener;
		private HashSet<ServerClient> _clients = new HashSet<ServerClient>();

		public int Port { get; }
		public bool IsOpen { get; private set; }

		public Server(int port) {
			Port = port;
			_tcpListener = new TcpListener(IPAddress.Any, port);
			_tcpListener.Start();
		}

		~Server() => Dispose(false);

		public void Dispose() {
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing) {
			if (_disposed) return;
			if (IsOpen) Close();
			// if (!disposing) return;
		}

		public void Start() {
			if (IsOpen) return;
			IsOpen = true;
			OnStart();
			Run().Wait();
			IsOpen = false;
		}

		public async Task StartAsync() {
			if (IsOpen) return;
			IsOpen = true;
			OnStart();
			await Run();
			IsOpen = false;
		}

		public void Close() {
			while (IsOpen) {}
			ServerClient[] copyClients = new ServerClient[_clients.Count];
			_clients.CopyTo(copyClients);
			foreach (ServerClient client in copyClients)
				DisconnectClient(client);
			OnClose();
			_tcpListener.Stop();
		}

		protected void DispatchEvents() {
			while (_tcpListener.Pending())
				ConnectClient(new ServerClient(this, _tcpListener.AcceptTcpClient()));
		}

		private void ConnectClient(ServerClient client) {
			if (AcceptClient(client)) {
				AddClient(client);
			} else {
				OnRefuseClient(client);
				client.Close();
			}
		}

		private void AddClient(ServerClient client) {
			client.Start();
			OnAcceptClient(client);
			_clients.Add(client);
		}

		private void DisconnectClient(ServerClient client) {
			if (!_clients.Contains(client)) return;
			OnDisconnectClient(client);
			client.Close();
			_clients.Remove(client);
		}

		public void SendAll(APacket packet) {
			foreach (ServerClient client in _clients)
				client.Send(packet);
		}

		public async Task SendAllAsync(APacket packet) {
			List<Task> taskList = new List<Task>(_clients.Count);
			foreach (ServerClient client in _clients)
				taskList.Add(client.SendAsync(packet));
			await Task.WhenAll(taskList);
		}

		protected virtual async Task Run() {
			while (IsOpen) {
				DispatchEvents();
				await Task.Yield();
			}
		}

		protected virtual void OnStart() { }
		protected virtual void OnClose() { }
		protected virtual bool AcceptClient(ServerClient client) => true;
		protected virtual void OnAcceptClient(ServerClient client) { }
		protected virtual void OnRefuseClient(ServerClient client) { }
		protected virtual void OnDisconnectClient(ServerClient client) { }
		protected virtual void OnReceive(ServerClient client) { }
	}
}