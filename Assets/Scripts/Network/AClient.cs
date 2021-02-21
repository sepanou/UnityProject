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
		private readonly Queue<APacket> _tcpQueue = new Queue<APacket>();
		private readonly Queue<APacket> _udpQueue = new Queue<APacket>();
		private readonly Thread _thread;

		public string Address { get; }
		public int TcpPort { get; }
		public int UdpPort { get; }
		public NetworkStream TcpStream { get; }
		public bool IsOpen { get; private set; }
		public bool Connected => _tcpClient.Connected;

		protected AClient(string address, int tcpPort, int udpPort): this(new TcpClient(address, tcpPort), udpPort) { }

		protected AClient(TcpClient tcpClient, int udpPort) {
			_tcpClient = tcpClient;
			IPEndPoint endPoint = (IPEndPoint)_tcpClient.Client.RemoteEndPoint;
			Address = endPoint.Address.ToString();
			TcpPort = endPoint.Port;
			TcpStream = _tcpClient.GetStream();
			
			_udpClient = new UdpClient(Address, udpPort);
			endPoint = (IPEndPoint)_udpClient.Client.RemoteEndPoint;
			UdpPort = endPoint.Port;
			
			_thread = new Thread(Run);
		}

		private void Run() {
			while (IsOpen) {
				if (_tcpQueue.Count > 0)
					_tcpQueue.Dequeue().Send(TcpStream);
				if (TcpStream.DataAvailable)
					OnReceive();
				if (!Connected)
					Disconnect();
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
			TcpStream.Dispose();
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
			=> _tcpQueue.Enqueue(packet);

		public void ReceiveTcp(APacket packet)
			=> packet.Receive(TcpStream);

		public TPacket ReceiveTcp<TPacket>() where TPacket: APacket, new() {
			TPacket packet = new TPacket();
			ReceiveTcp(packet);
			return packet;
		}

		protected virtual void OnStart() { }
		protected virtual void OnClose() { }
		protected virtual bool OnDisconnect() => false;
		protected virtual void OnReceive() { }
	}
}