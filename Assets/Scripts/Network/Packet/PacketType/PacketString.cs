using System.Text;

namespace Network.Packet.PacketType {
	public sealed class PacketString: PacketType<string> {
		public PacketString() {}
		public PacketString(byte[] bytes): base(bytes) { }
		public PacketString(string data) { WriteType(data); }
		
		public override string ReadType()
			=> Encoding.UTF8.GetString(Read());

		public override void WriteType(string data)
			=> Write(Encoding.UTF8.GetBytes(data));
	}
}