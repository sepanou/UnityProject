using System.Collections.Generic;
using Network;
using Network.Packet.PacketType;
using UnityEngine;
using UnityEngine.UI;

public class TestNetwork: MonoBehaviour {
	public InputField serverInputField;
	public Text serverText;
	public InputField clientInputField;
	public Text clientText;

	private void LogServer(string str) => _serverQueue.Enqueue(str);
	private void LogClient(string str) => _clientQueue.Enqueue(str);
	
	public void SendServer() => _server.SendAllTcp(new PacketString(serverInputField.text));
	public void SendClient() => _client.SendTcp(new PacketString(clientInputField.text));
	
	private class MyClient: AClient {
		private readonly TestNetwork _master;

		public MyClient(TestNetwork master, string address, int tcpPort, int udpPort) : base(address, tcpPort, udpPort) { _master = master; }
		protected override void OnStart() {
			_master.LogClient($"Connected with {Address} on port {TcpPort}");
		}

		protected override void OnClose() {
			_master.LogClient("Closed");
		}

		protected override void OnReceive() {
			ReceiveTcp<PacketString>().Read(out string message);
			_master.LogClient($"Received '{message}'");
		}
	}
	
	private class MyServer: AServer {
		private readonly TestNetwork _master;
		
		public MyServer(TestNetwork master, int port): base(port) { _master = master; }

		protected override void OnStart() {
			_master.LogServer($"Started on port {TcpPort}");
		}

		protected override void OnClose() {
			_master.LogServer("Closed");
		}

		protected override void OnAcceptClient(ServerClient client) {
			_master.LogServer($"Accepted a connection from {client.Address}:{client.TcpPort}");
		}

		protected override void OnRefuseClient(ServerClient client) {
			_master.LogServer($"Refused a connection from {client.Address}:{client.TcpPort}");
		}

		protected override bool OnDisconnectClient(ServerClient client) {
			_master.LogServer($"{client.Address}:{client.TcpPort} Got disconnected");
			return false;
		}

		protected override void OnReceive(ServerClient client) {
			client.ReceiveTcp<PacketString>().Read(out string message);
			_master.LogServer($"{client.Address}:{client.TcpPort} Sent '{message}'");
		}
	}

	private MyServer _server;
	private MyClient _client;

	private Queue<string> _serverQueue = new Queue<string>();
	private Queue<string> _clientQueue = new Queue<string>();

	private void Start() {
		_server = new MyServer(this, 42069);
		_server.Start();
		
		_client = new MyClient(this, "localhost", 42069, 42070);
		_client.Start();
	}

	private void Update() {
		while (_serverQueue.Count > 0)
			serverText.text += _serverQueue.Dequeue() + "\n";
		while (_clientQueue.Count > 0)
			clientText.text += _clientQueue.Dequeue() + "\n";
	}

	private void OnApplicationQuit() {
		_client.ForceClose();
		_server.ForceClose();
	}
}