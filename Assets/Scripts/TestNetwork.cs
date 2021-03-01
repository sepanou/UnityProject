using System.Collections.Generic;
using System.Net;

using UnityEngine;
using UnityEngine.UI;

using Network;
using Network.Packet.Packets;
using Network.Packet.PacketType;

public class TestNetwork: MonoBehaviour {
	public InputField serverInputField;
	public Text serverText;
	public InputField clientInputField;
	public Text clientText;

	private static readonly System.Random Random = new System.Random();

	private void LogServer(string str) => _serverQueue.Enqueue(str);
	private void LogClient(string str) => _clientQueue.Enqueue(str);
	
	public void SendServer() {
		Packets packet = new Packets();
		packet.Add(new PacketInt(Random.Next()));
		packet.Add(new PacketString(serverInputField.text));
		_server.SendAllTcp(packet);
	}

	public void SendClient() {
		Packets packet = new Packets();
		packet.Add(new PacketInt(Random.Next()));
		packet.Add(new PacketString(clientInputField.text));
		_client.SendTcp(packet);
	}
	
	private class MyClient: AClient {
		private readonly TestNetwork _master;

		public MyClient(TestNetwork master, IPEndPoint endPoint) : base(endPoint) { _master = master; }
		
		protected override void OnStart()
			=> _master.LogClient($"Connected with {EndPoint}");

		protected override void OnClose()
			=> _master.LogClient("Closed");

		protected override void OnReceive() {
			Packets packet = Receive<Packets>();
			int r = packet.Get<PacketInt>().ReadType();
			string s = packet.Get<PacketString>().ReadType();
			_master.LogClient($"Received {r} and '{s}'");
		}
	}
	
	private class MyServer: AServer {
		private readonly TestNetwork _master;
		
		public MyServer(TestNetwork master, IPEndPoint endPoint): base(endPoint) { _master = master; }

		protected override void OnStart()
			=> _master.LogServer($"Started on {EndPoint}");

		protected override void OnClose()
			=> _master.LogServer("Closed");

		protected override void OnAcceptClient(ServerClient client)
			=> _master.LogServer($"Accepted a connection from {client.EndPoint}");

		protected override void OnRefuseClient(ServerClient client)
			=> _master.LogServer($"Refused a connection from {client.EndPoint}");

		protected override bool OnDisconnectClient(ServerClient client) {
			_master.LogServer($"{client.EndPoint} Got disconnected");
			return false;
		}

		protected override void OnReceive(ServerClient client) {
			Packets packet = client.Receive<Packets>();
			int r = packet.Get<PacketInt>().ReadType();
			string s = packet.Get<PacketString>().ReadType();
			_master.LogServer($"Received {r} and '{s}' from {client.EndPoint}");
		}
	}

	private MyServer _server;
	private MyClient _client;

	private readonly Queue<string> _serverQueue = new Queue<string>();
	private readonly Queue<string> _clientQueue = new Queue<string>();

	private void Start() {
		_server = new MyServer(this, new IPEndPoint(IPAddress.Any, 42069));
		_server.Start();
		
		_client = new MyClient(this, new IPEndPoint(IPAddress.Loopback, _server.EndPoint.Port));
		_client.Start();
	}

	private void Update() {
		while (_serverQueue.Count > 0)
			serverText.text += _serverQueue.Dequeue() + "\n";
		while (_clientQueue.Count > 0)
			clientText.text += _clientQueue.Dequeue() + "\n";
	}

	private void OnApplicationQuit() {
		_client.Close();
		_server.Close();
	}
}