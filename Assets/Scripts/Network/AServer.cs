using IDisposable = System.IDisposable;
using GC = System.GC;
using ClientDict = System.Collections.Generic.Dictionary<System.Net.IPEndPoint, Network.AServer.ServerClient>;
using IPEndPoint = System.Net.IPEndPoint;
using TcpClient = System.Net.Sockets.TcpClient;
using TcpListener = System.Net.Sockets.TcpListener;
using Thread = System.Threading.Thread;
using APacket = Network.Packet.APacket;

namespace Network {
	/// <summary>
	/// abstract class defining the behaviour of a server
	/// listens for connecting clients and manage network through special clients (<see cref="ServerClient"/>)
	/// interface between packets and clients
	/// implements <see cref="IDisposable"/>
	/// </summary>
	public abstract class AServer: IDisposable {
		/// <summary> implements <see cref="AClient"/> for servers' usage </summary>
		public class ServerClient: AClient {
			/// <summary> the client's server </summary>
			private readonly AServer _server;
			/// <summary> constructs a client from server and TCP client </summary>
			/// <param name="server"> the client's server </param>
			/// <param name="tcpClient"> the client's TCP client </param>
			public ServerClient(AServer server, TcpClient tcpClient): base(tcpClient) { _server = server; }
			/// <summary> redirect <see cref="AClient.OnDisconnect"/> event to server </summary>
			/// <returns> should the client be disconnected ? </returns>
			protected override bool OnDisconnect() => _server.DisconnectClient(this);
			/// <summary> redirect <see cref="AClient.OnReceive"/> event to server </summary>
			protected override void OnReceive() => _server.OnReceive(this);
		}

		/// <summary> has the object already been disposed of ? </summary>
		private bool _disposed = false;
		/// <summary> listens for TCP connections </summary>
		private readonly TcpListener _tcpListener;
		/// <summary> server's execution thread <seealso cref="Run"/> </summary>
		private readonly Thread _thread;
		/// <summary> dictionary of all the server's clients </summary>
		private readonly ClientDict _clients = new ClientDict();

		/// <summary> server's listening IP address </summary>
		public readonly IPEndPoint EndPoint;
		/// <summary> is the server running ? </summary>
		public bool IsOpen { get; private set; }
		/// <summary> collection of all the server's client for outside access </summary>
		public ClientDict.ValueCollection Clients => _clients.Values;

		/// <summary> constructs a server from a listening address </summary>
		/// <param name="endPoint"> server's listening address </param>
		protected AServer(IPEndPoint endPoint) {
			_tcpListener = new TcpListener(endPoint);
			EndPoint = (IPEndPoint)_tcpListener.LocalEndpoint;
			_tcpListener.Start();
			_thread = new Thread(Run);
		}
		
		/// <summary>
		/// function ran by the server's thread <see cref="_thread"/>
		/// listens for connections and creates clients
		/// </summary>
		private void Run() {
			while (IsOpen) {
				if (_tcpListener.Pending())
					ConnectClient(new ServerClient(this, _tcpListener.AcceptTcpClient()));
			}
		}

		/// <summary> custom destructor to implement <see cref="IDisposable"/> </summary>
		~AServer() => Dispose(false);

		/// <summary> implementing <see cref="IDisposable"/> </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary> implementing <see cref="IDisposable"/> </summary>
		protected virtual void Dispose(bool disposing) {
			if (_disposed) return;
			_disposed = true;
			if (IsOpen) Close();
			if (!disposing) return;
			foreach (ServerClient client in _clients.Values)
				client.Dispose();
		}

		/// <summary> starts the server's thread </summary>
		public void Start() {
			if (IsOpen) return;
			IsOpen = true;
			_thread.Start();
			OnStart();
		}

		/// <summary>
		/// closes the server's thread
		/// <seealso cref="ForceClose"/>
		/// </summary>
		public void Close() {
			if (!IsOpen) return;
			OnClose();
			IsOpen = false;
			_thread.Join();
			foreach (ServerClient client in _clients.Values)
				client.Close();
			_tcpListener.Stop();
		}

		/// <summary>
		/// forces the server's thread to close
		/// <seealso cref="Close"/>
		/// </summary>
		public void ForceClose() {
			if (!IsOpen) return;
			IsOpen = false;
			_thread.Interrupt();
			foreach (ServerClient client in _clients.Values)
				client.ForceClose();
			_tcpListener.Stop();
		}

		/// <summary> manage client connections </summary>
		/// <param name="client"> client to try and connect </param>
		private void ConnectClient(ServerClient client) {
			if (AcceptClient(client)) {
				OnAcceptClient(client);
				AddClient(client);
			} else {
				OnRefuseClient(client);
				client.Close();
			}
		}

		/// <summary>
		/// adds the client to the server's dict (<see cref="ClientDict"/>)
		/// and starts it with <see cref="ServerClient.Start"/>
		/// <seealso cref="RemoveClient"/>
		/// </summary>
		/// <param name="client"> client to add </param>
		private void AddClient(ServerClient client) {
			_clients.Add(client.EndPoint, client);
			client.Start();
		}

		/// <summary> called on connection loss, try and reconnect or disconnects client </summary>
		/// <param name="client"> lost client </param>
		/// <returns> has the client been disconnected ? </returns>
		private bool DisconnectClient(ServerClient client) {
			if (!_clients.ContainsKey(client.EndPoint)) return false;
			bool result = OnDisconnectClient(client);
			if (!result) RemoveClient(client);
			return result;
		}

		/// <summary>
		/// removes the client from the server's dict (<see cref="ClientDict"/>)
		/// and closes it with <see cref="ServerClient.Close"/>
		/// <seealso cref="RemoveClient"/>
		/// </summary>
		/// <param name="client"> client to remove </param>
		private void RemoveClient(ServerClient client) {
			if (!_clients.ContainsKey(client.EndPoint)) return;
			client.Close();
			_clients.Remove(client.EndPoint);
		}

		/// <summary> send packet to all clients with TCP </summary>
		/// <param name="packet"> packet to send </param>
		public void SendAllTcp(APacket packet) {
			foreach (ServerClient client in _clients.Values)
				client.SendTcp(packet);
		}
		
		/// <summary> send packet to all clients with UDP </summary>
		/// <param name="packet"> packet to send </param>
		public void SendAllUdp(APacket packet) {
			foreach (ServerClient client in _clients.Values)
				client.SendUdp(packet);
		}
		
		/// <summary> send packet to a specific clients with TCP </summary>
		/// <param name="packet"> packet to send </param>
		/// <param name="endPoint"> address of the remote client </param>
		public void SendTcp(APacket packet, IPEndPoint endPoint)
			=> _clients[endPoint].SendTcp(packet);
		
		/// <summary> send packet to a specific clients with UDP </summary>
		/// <param name="packet"> packet to send </param>
		/// <param name="endPoint"> address of the remote client </param>
		public void SendUdp(APacket packet, IPEndPoint endPoint)
			=> _clients[endPoint].SendUdp(packet);

		/// <summary> on start event <seealso cref="Start"/> </summary>
		protected virtual void OnStart() { }
		/// <summary> on close event <seealso cref="Close"/> </summary>
		protected virtual void OnClose() { }
		/// <summary> accept client event <seealso cref="ConnectClient"/> </summary>
		/// <param name="client"> client to accept </param>
		/// <returns> should the client be accepted ? </returns>
		protected virtual bool AcceptClient(ServerClient client) => true;
		/// <summary> on accept event <seealso cref="ConnectClient"/> </summary>
		/// <param name="client"> accepted client </param>
		protected virtual void OnAcceptClient(ServerClient client) { }
		/// <summary> on refuse event <seealso cref="ConnectClient"/> </summary>
		/// <param name="client"> refused client </param>
		protected virtual void OnRefuseClient(ServerClient client) { }
		/// <summary> on disconnect event <seealso cref="DisconnectClient"/> </summary>
		/// <param name="client"> lost client </param>
		protected virtual bool OnDisconnectClient(ServerClient client) => false;
		/// <summary> on receive event <seealso cref="ServerClient.Receive"/> </summary>
		/// <param name="client"> client who received packet </param>
		protected virtual void OnReceive(ServerClient client) { }
	}
}