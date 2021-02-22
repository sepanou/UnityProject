using IDisposable = System.IDisposable;
using GC = System.GC;
using Array = System.Array;
using BytesStack = System.Collections.Generic.Stack<byte[]>;
using IPEndPoint = System.Net.IPEndPoint;
using TcpClient = System.Net.Sockets.TcpClient;
using UdpClient = System.Net.Sockets.UdpClient;
using NetworkStream = System.Net.Sockets.NetworkStream;
using IOControlCode = System.Net.Sockets.IOControlCode;
using Thread = System.Threading.Thread;
using APacket = Network.Packet.APacket;

namespace Network {
	/// <summary>
	/// abstract class defining the behaviour of a client
	/// interface between packets and network
	/// implements <see cref="IDisposable"/>
	/// </summary>
	public abstract class AClient: IDisposable {
		/// <summary> has the object already been disposed of ? </summary>
		private bool _disposed;
		/// <summary> TCP client </summary>
		private readonly TcpClient _tcpClient;
		/// <summary> UDP client </summary>
		private readonly UdpClient _udpClient;
		/// <summary> buffer for receiving TCP packets </summary>
		private readonly byte[] _tcpBuffer = new byte[2048];
		/// <summary> TCP client's stream </summary>
		private readonly NetworkStream _tcpStream;
		/// <summary> stack for packets to send with TCP </summary>
		private readonly BytesStack _tcpStack = new BytesStack();
		/// <summary> stack for packets to send with UDP </summary>
		private readonly BytesStack _udpStack = new BytesStack();
		/// <summary> stack for received packets </summary>
		private readonly BytesStack _receiveStack = new BytesStack();
		/// <summary> client's execution thread <seealso cref="Run"/> </summary>
		private readonly Thread _thread;

		/// <summary> contains IP address of the remote host </summary>
		public IPEndPoint EndPoint { get; }
		/// <summary> is the client running ? </summary>
		public bool IsOpen { get; private set; }
		/// <summary> is the client connected to a remote host ? </summary>
		public bool Connected => _tcpClient.Connected;
		/// <summary> are packets available ? </summary>
		public bool Available => _tcpClient.Available != 0 || _udpClient.Available != 0;

		/// <summary> creates and connects a TCP client from an address </summary>
		/// <param name="endPoint"> address of the remote host </param>
		/// <returns> the created TCP client </returns>
		private static TcpClient TcpClientFromEndPoint(IPEndPoint endPoint) {
			TcpClient client = new TcpClient();
			client.Connect(endPoint);
			return client;
		}

		/// <summary> constructs a client from a remote address </summary>
		/// <param name="endPoint"> address of the remote host </param>
		protected AClient(IPEndPoint endPoint): this(TcpClientFromEndPoint(endPoint)) { }
		
		/// <summary> constructs a client from a TCP client </summary>
		/// <param name="tcpClient"> TCP client </param>
		protected AClient(TcpClient tcpClient) {
			_tcpClient = tcpClient;
			EndPoint = (IPEndPoint)_tcpClient.Client.RemoteEndPoint;
			_tcpStream = _tcpClient.GetStream();
			
			_udpClient = new UdpClient();
			_udpClient.Client.IOControl(
				// SIO_UDP_CONNRESET
				(IOControlCode)(-1744830452),
				new byte[] { 0, 0, 0, 0 },
				null
			);
			_udpClient.Connect(EndPoint);
			
			_thread = new Thread(Run);
		}

		/// <summary>
		/// function ran by the client's thread <see cref="_thread"/>
		/// receives and sends packets
		/// future: track disconnections (keep alive packets)
		/// </summary>
		private void Run() {
			while (IsOpen) {
				if (_tcpStack.Count > 0) {
					byte[] bytes = _tcpStack.Pop();
					_tcpStream.Write(bytes, 0, bytes.Length);
				}
				
				if (_udpStack.Count > 0) {
					byte[] bytes = _udpStack.Pop();
					_udpClient.Send(bytes, bytes.Length);
				}

				if (_tcpStream.DataAvailable) {
					byte[] bytes = new byte[_tcpStream.Read(_tcpBuffer, 0, _tcpBuffer.Length)];
					Array.Copy(_tcpBuffer, bytes, bytes.Length);
					_receiveStack.Push(bytes);
					OnReceive();
				}

				if (_udpClient.Available > 0) {
					IPEndPoint endPoint = EndPoint;
					_receiveStack.Push(_udpClient.Receive(ref endPoint));
					OnReceive();
				}
			}
		}

		/// <summary> custom destructor to implement <see cref="IDisposable"/> </summary>
		~AClient() => Dispose(false);
		
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
			_tcpClient.Dispose();
			_udpClient.Dispose();
			_tcpStream.Dispose();
		}

		/// <summary> starts the client's thread </summary>
		public void Start() {
			if (IsOpen) return;
			IsOpen = true;
			OnStart();
			_thread.Start();
		}

		/// <summary>
		/// closes the client's thread
		/// <seealso cref="ForceClose"/>
		/// </summary>
		public void Close() {
			if (!IsOpen) return;
			OnClose();
			IsOpen = false;
			_thread.Join();
			_tcpClient.Close();
			_udpClient.Close();
		}

		/// <summary>
		/// forces the client's thread to close
		/// <seealso cref="Close"/>
		/// </summary>
		public void ForceClose() {
			if (!IsOpen) return;
			IsOpen = false;
			_thread.Interrupt();
			_tcpClient.Close();
			_udpClient.Close();
		}

		/// <summary> tries to reconnect to a lost host and closes if failure to do so </summary>
		private void Disconnect() {
			if (!OnDisconnect())
				Close();
		}

		/// <summary> send a packet with TCP </summary>
		/// <param name="packet"> packet to send </param>
		public void SendTcp(APacket packet)
			=> _tcpStack.Push(packet.Read());
		
		/// <summary> send a packet with UDP </summary>
		/// <param name="packet"> packet to send </param>
		public void SendUdp(APacket packet)
			=> _udpStack.Push(packet.Read());

		public APacket Receive(APacket packet) {
			packet.Write(_receiveStack.Pop());
			return packet;
		}

		public TPacket Receive<TPacket>() where TPacket: APacket, new()
			=> (TPacket)Receive(new TPacket());

		/// <summary> on start event <seealso cref="Start"/> </summary>
		protected virtual void OnStart() { }
		/// <summary> on close event <seealso cref="Close"/> </summary>
		protected virtual void OnClose() { }
		/// <summary> on disconnect event <seealso cref="Disconnect"/> </summary>
		protected virtual bool OnDisconnect() => false;
		/// <summary> on receive event <seealso cref="Receive"/> </summary>
		protected virtual void OnReceive() { }
	}
}