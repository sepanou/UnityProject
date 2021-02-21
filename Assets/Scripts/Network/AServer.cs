using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Network.Packet;
using ClientDict = System.Collections.Generic.Dictionary<System.Net.IPEndPoint, Network.AServer.ServerClient>;

namespace Network {
	public abstract class AServer: IDisposable {
		public class ServerClient: AClient {
			private readonly AServer _server;
			public ServerClient(AServer server, TcpClient tcpClient): base(tcpClient) { _server = server; }
			protected override bool OnDisconnect() => _server.DisconnectClient(this);
			protected override void OnReceive() => _server.OnReceive(this);
		}

		private bool _disposed = false;
		private readonly TcpListener _tcpListener;
		private readonly Thread _thread;
		private readonly ClientDict _clients = new ClientDict();

		public readonly IPEndPoint EndPoint;
		public bool IsOpen { get; private set; }
		public ClientDict.ValueCollection Clients => _clients.Values;

		protected AServer(IPEndPoint endPoint) {
			_tcpListener = new TcpListener(endPoint);
			EndPoint = (IPEndPoint)_tcpListener.LocalEndpoint;
			_thread = new Thread(Run);
		}

		private void Run() {
			while (IsOpen) {
				if (_tcpListener.Pending())
					ConnectClient(new ServerClient(this, _tcpListener.AcceptTcpClient()));
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
			foreach (ServerClient client in _clients.Values)
				client.Dispose();
		}

		public void Start() {
			if (IsOpen) return;
			IsOpen = true;
			_tcpListener.Start();
			_thread.Start();
			OnStart();
		}

		public void Close() {
			if (!IsOpen) return;
			OnClose();
			IsOpen = false;
			_thread.Join();
			foreach (ServerClient client in _clients.Values)
				client.Close();
			_tcpListener.Stop();
		}

		public void ForceClose() {
			if (!IsOpen) return;
			IsOpen = false;
			_thread.Interrupt();
			foreach (ServerClient client in _clients.Values)
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
			_clients.Add(client.EndPoint, client);
			client.Start();
		}

		private bool DisconnectClient(ServerClient client) {
			if (!_clients.ContainsKey(client.EndPoint)) return false;
			bool result = OnDisconnectClient(client);
			if (!result) RemoveClient(client);
			return result;
		}

		private void RemoveClient(ServerClient client) {
			if (!_clients.ContainsKey(client.EndPoint)) return;
			client.Close();
			_clients.Remove(client.EndPoint);
		}

		public void SendAllTcp(APacket packet) {
			foreach (ServerClient client in _clients.Values)
				client.SendTcp(packet);
		}

		public void SendAllUdp(APacket packet) {
			foreach (ServerClient client in _clients.Values)
				client.SendUdp(packet);
		}

		public void SendTcp(APacket packet, IPEndPoint endPoint)
			=> _clients[endPoint].SendTcp(packet);

		public void SendUdp(APacket packet, IPEndPoint endPoint)
			=> _clients[endPoint].SendUdp(packet);

		protected virtual void OnStart() { }
		protected virtual void OnClose() { }
		protected virtual bool AcceptClient(ServerClient client) => true;
		protected virtual void OnAcceptClient(ServerClient client) { }
		protected virtual void OnRefuseClient(ServerClient client) { }
		protected virtual bool OnDisconnectClient(ServerClient client) => false;
		protected virtual void OnReceive(ServerClient client) { }
	}
}