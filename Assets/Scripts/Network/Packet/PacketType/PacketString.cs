using System.Text;

namespace Network.Packet.PacketType {
	public sealed class PacketString : PacketType<string> {
		public PacketString() { }
		public PacketString(string data) => Write(data);

		public override void Read(out string data) {
			data = Encoding.UTF8.GetString(Read());
		}

		public override void Write(string data) {
			Write(Encoding.UTF8.GetBytes(data));
		}
	}
}