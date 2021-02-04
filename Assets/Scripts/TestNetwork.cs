using Network;
using Network.Packet.PacketType;
using UnityEngine;
using UnityEngine.UI;

public class TestNetwork: MonoBehaviour {
	public InputField serverInputField;
	public Text serverText;
	public InputField clientInputField;
	public Text clientText;

	private void LogServer(string str) => serverText.text += str + "\n";
	private void LogClient(string str) => clientText.text += str + "\n";
	
	public void SendServer() => _server.SendAll(new PacketString(serverInputField.text));
	public void SendClient() => _client.Send(new PacketString(clientInputField.text));
	
	private class MyClient: Client {
		private readonly TestNetwork _master;

		public MyClient(TestNetwork master, string address, int port) : base(address, port) { _master = master; }
		protected override void OnStart() {
			_master.LogClient($"Client: connected with {Address} on port {Port}");
		}

		protected override void OnClose() {
			_master.LogClient("Client: closed");
		}

		protected override void OnReceive() {
			PacketString packet = Receive<PacketString>();
			packet.Read(out string message);
			_master.LogClient($"Client: received '{message}'");
		}
	}
	
	private class MyServer: Server {
		private readonly TestNetwork _master;
		
		public MyServer(TestNetwork master, int port): base(port) { _master = master; }

		protected override void OnStart() {
			_master.LogServer($"Server: started on port {Port}");
		}

		protected override void OnClose() {
			_master.LogServer("Server: closed");
		}

		protected override void OnAcceptClient(ServerClient client) {
			_master.LogServer($"Server: accepted a connection from {client.Address}:{client.Port}");
		}

		protected override void OnRefuseClient(ServerClient client) {
			_master.LogServer($"Server: refused a connection from {client.Address}:{client.Port}");
		}

		protected override void OnDisconnectClient(ServerClient client) {
			_master.LogServer($"Server: {client.Address}:{client.Port} got disconnected");
		}

		protected override void OnReceive(ServerClient client) {
			PacketString packet = client.Receive<PacketString>();
			packet.Read(out string message);
			_master.LogServer($"Server: {client.Address}:{client.Port} sent '{message}'");
		}
	}

	private MyServer _server;
	private MyClient _client;

	public void Start() {
		_server = new MyServer(this, 42069);
		_server.Start();
		
		_client = new MyClient(this, "localhost", 42069);
		_client.Start();
	}
}