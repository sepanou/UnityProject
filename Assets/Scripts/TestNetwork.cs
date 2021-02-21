using System.Collections.Generic;
using System.Net;
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
	
	public void SendServer() => _server.SendAllTcp(PacketString.Make<PacketString>(serverInputField.text));
	public void SendClient() => _client.SendTcp(PacketString.Make<PacketString>(clientInputField.text));
	
	private class MyClient: AClient {
		private readonly TestNetwork _master;

		public MyClient(TestNetwork master, IPEndPoint endPoint) : base(endPoint) { _master = master; }
		
		protected override void OnStart()
			=> _master.LogClient($"Connected with {EndPoint}");

		protected override void OnClose()
			=> _master.LogClient("Closed");

		protected override void OnReceive()
			=> _master.LogClient($"Received '{Receive<PacketString>().ReadType()}'");
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

		protected override void OnReceive(ServerClient client)
			=> _master.LogServer($"{client.EndPoint} Sent '{client.Receive<PacketString>().ReadType()}'");
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