using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Network.Packet;
using ClientSet = System.Collections.Generic.HashSet<Network.AServer.ServerClient>;

namespace Network {
	public abstract class AServer: IDisposable {
		public class ServerClient: AClient {
			private readonly AServer _server;
			public ServerClient(AServer server, TcpClient tcpClient, int udpPort): base(tcpClient, udpPort) { _server = server; }
			protected override bool OnDisconnect() => _server.DisconnectClient(this);
			protected override void OnReceive() => _server.OnReceive(this);
		}

		private bool _disposed = false;
		private readonly TcpListener _tcpListener;
		private readonly Thread _thread;
		private readonly ClientSet _clients = new ClientSet();

		public int TcpPort { get; }
		public bool IsOpen { get; private set; }
		public ClientSet.Enumerator Clients => _clients.GetEnumerator();

		protected AServer(int tcpPort) {
			TcpPort = tcpPort;
			_tcpListener = new TcpListener(IPAddress.Any, tcpPort);
			_tcpListener.Start();
			_thread = new Thread(Run);
		}

		private void Run() {
			while (IsOpen) {
				if (_tcpListener.Pending())
					ConnectClient(new ServerClient(this, _tcpListener.AcceptTcpClient(), 0));
			}
		}

		~AServer() => Dispose(false);

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (_disposed) return;
			_disposed = true;
			if (IsOpen) Close();
			if (!disposing) return;
			foreach (ServerClient client in _clients)
				client.Dispose();
		}

		public void Start() {
			if (IsOpen) return;
			IsOpen = true;
			OnStart();
			_thread.Start();
		}

		public void Close() {
			if (!IsOpen) return;
			IsOpen = false;
			_thread.Join();
			foreach (ServerClient client in _clients)
				client.Close();
			_tcpListener.Stop();
			OnClose();
		}

		public void ForceClose() {
			if (!IsOpen) return;
			IsOpen = false;
			_thread.Interrupt();
			foreach (ServerClient client in _clients)
				client.ForceClose();
			_tcpListener.Stop();
		}

		private void ConnectClient(ServerClient client) {
			if (AcceptClient(client)) {
				OnAcceptClient(client);
				AddClient(client);
			} else {
				OnRefuseClient(client);
				client.Close();
			}
		}

		private void AddClient(ServerClient client) {
			client.Start();
			_clients.Add(client);
		}

		private bool DisconnectClient(ServerClient client) {
			if (!_clients.Contains(client)) return false;
			bool result = OnDisconnectClient(client);
			if (!result) RemoveClient(client);
			return result;
		}

		private void RemoveClient(ServerClient client) {
			if (!_clients.Contains(client)) return;
			client.Close();
			_clients.Remove(client);
		}

		public void SendAllTcp(APacket packet) {
			foreach (ServerClient client in _clients)
				client.SendTcp(packet);
		}

		protected virtual void OnStart() { }
		protected virtual void OnClose() { }
		protected virtual bool AcceptClient(ServerClient client) => true;
		protected virtual void OnAcceptClient(ServerClient client) { }
		protected virtual void OnRefuseClient(ServerClient client) { }
		protected virtual bool OnDisconnectClient(ServerClient client) => false;
		protected virtual void OnReceive(ServerClient client) { }
	}
}