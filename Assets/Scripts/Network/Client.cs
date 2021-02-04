using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Network.Packet;

namespace Network {
	public class Client: IDisposable {
		private bool _disposed;
		private readonly TcpClient _tcpClient;

		public string Address { get; }
		public int Port { get; }
		public NetworkStream Stream { get; }
		public bool IsOpen { get; private set; }
		public bool IsDataAvailable => Stream.DataAvailable;
		public int DataAvailable => _tcpClient.Available;
		public bool Connected => _tcpClient.Connected;

		public Client(string address, int port) {
			Address = address;
			Port = port;
			_tcpClient = new TcpClient(address, port);
			Stream = _tcpClient.GetStream();
		}

		public Client(TcpClient tcpClient) {
			Address = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
			Port = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Port;
			_tcpClient = tcpClient;
			Stream = _tcpClient.GetStream();
		}

		~Client() => Dispose(false);
		
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		protected virtual void Dispose(bool disposing) {
			if (_disposed) return;
			if (IsOpen)
				Close();
			if (disposing) {
				_tcpClient.Dispose();
				Stream.Dispose();
			}
			_disposed = true;
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
			while (IsOpen) { }
			OnClose();
			_tcpClient.Close();
		}

		protected void DispatchEvents() {
			if (IsDataAvailable)
				OnReceive();
		}

		private void Disconnect() {
			OnDisconnect();
			Close();
		}

		public void Send(APacket packet) {
			packet.Send(Stream);
		}

		public async Task SendAsync(APacket packet) {
			await packet.SendAsync(Stream);
		}

		public void Receive(APacket packet) {
			packet.Receive(Stream);
		}

		public TPacket Receive<TPacket>() where TPacket: APacket, new() {
			TPacket packet = new TPacket();
			Receive(packet);
			return packet;
		}

		public async Task ReceiveAsync(APacket packet) {
			await packet.ReceiveAsync(Stream);
		}

		public async Task<TPacket> ReceiveAsync<TPacket>() where TPacket: APacket, new() {
			TPacket packet = new TPacket();
			await ReceiveAsync(packet);
			return packet;
		}

		protected virtual async Task Run() {
			while (IsOpen) {
				DispatchEvents();
				await Task.Yield();
			}
		}

		protected virtual void OnStart() { }
		protected virtual void OnClose() { }
		protected virtual void OnDisconnect() { }
		protected virtual void OnReceive() { }
	}
}