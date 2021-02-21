using System.Text;

namespace Network.Packet.PacketType {
	public sealed class PacketString: PacketType<string> {
		public override string ReadType()
			=> Encoding.UTF8.GetString(Read());

		public override void WriteType(string data)
			=> Write(Encoding.UTF8.GetBytes(data));
	}
}