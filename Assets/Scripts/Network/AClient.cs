using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Network.Packet;

namespace Network {
	public abstract class AClient: IDisposable {
		private bool _disposed;
		private readonly TcpClient _tcpClient;
		private readonly UdpClient _udpClient;
		private readonly byte[] _tcpBuffer = new byte[2048];
		private readonly NetworkStream _tcpStream;
		private readonly Stack<byte[]> _tcpStack = new Stack<byte[]>();
		private readonly Stack<byte[]> _udpStack = new Stack<byte[]>();
		private readonly Stack<byte[]> _receiveStack = new Stack<byte[]>();
		private readonly Thread _thread;

		public IPEndPoint EndPoint { get; }
		public bool IsOpen { get; private set; }
		public bool Connected => _tcpClient.Connected;
		public bool Available => _tcpClient.Available != 0 || _udpClient.Available != 0;

		private static TcpClient TcpClientFromEndPoint(IPEndPoint endPoint) {
			TcpClient client = new TcpClient();
			client.Connect(endPoint);
			return client;
		}

		protected AClient(IPEndPoint endPoint): this(TcpClientFromEndPoint(endPoint)) { }

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

		~AClient() => Dispose(false);
		
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		protected virtual void Dispose(bool disposing) {
			if (_disposed) return;
			_disposed = true;
			if (IsOpen) Close();
			if (!disposing) return;
			_tcpClient.Dispose();
			_udpClient.Dispose();
			_tcpStream.Dispose();
		}

		public void Start() {
			if (IsOpen) return;
			IsOpen = true;
			OnStart();
			_thread.Start();
		}

		public void Close() {
			if (!IsOpen) return;
			OnClose();
			IsOpen = false;
			_thread.Join();
			_tcpClient.Close();
			_udpClient.Close();
		}

		public void ForceClose() {
			if (!IsOpen) return;
			IsOpen = false;
			_thread.Interrupt();
			_tcpClient.Close();
			_udpClient.Close();
		}

		private void Disconnect() {
			if (!OnDisconnect())
				Close();
		}

		public void SendTcp(APacket packet)
			=> _tcpStack.Push(packet.Read());

		public void SendUdp(APacket packet)
			=> _udpStack.Push(packet.Read());

		public APacket Receive(APacket packet) {
			packet.Write(_receiveStack.Pop());
			return packet;
		}

		public TPacket Receive<TPacket>() where TPacket: APacket, new()
			=> (TPacket)Receive(new TPacket());

		protected virtual void OnStart() { }
		protected virtual void OnClose() { }
		protected virtual bool OnDisconnect() => false;
		protected virtual void OnReceive() { }
	}
}