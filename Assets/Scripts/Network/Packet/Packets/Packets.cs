using Network.Packet.PacketType;

namespace Network.Packet.Packets {
	public class Packets: APacket {
		public void Add(APacket packet) {
			if (!packet.GetFixedSize().HasValue)
				Write(PacketUShort.Make((ushort)packet.Length).Read());
			Write(packet.Read());
		}
		
		public TPacket Get<TPacket>() where TPacket: APacket, new() {
			TPacket packet = new TPacket();
			int? fixedSize = packet.GetFixedSize();
			if (fixedSize.HasValue)
				packet.Write(Read(fixedSize.Value));
			else {
				PacketUShort i = new PacketUShort();
				i.Write(Read(sizeof(ushort)));
				packet.Write(Read(i.ReadType()));
			}
			return packet;
		}
	}
}