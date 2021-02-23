using Network.Packet.PacketType;

namespace Network.Packet.Packets {
	public class Packets: APacket {
		public Packets() {}
		public Packets(byte[] bytes): base(bytes) { }
		
		public void Add(APacket packet) {
			if (!packet.GetFixedSize().HasValue)
				Write(new PacketUShort((ushort)packet.Length).Read());
			Write(packet.Read());
		}
		
		public TPacket Get<TPacket>() where TPacket: APacket, new() {
			TPacket packet = new TPacket();
			int? fixedSize = packet.GetFixedSize();
			packet.Write(
				fixedSize.HasValue
				? Read(fixedSize.Value)
				: Read(new PacketUShort(Read(sizeof(ushort))).ReadType()))
			;
			return packet;
		}
	}
}