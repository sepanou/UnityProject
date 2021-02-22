using Network.Packet.PacketType;

namespace Network.Packet.Packets {
	public class Packets: APacket {
		public void Add(APacket packet) {
			if (!packet.GetFixedSize().HasValue)
				Write(PacketInt.Make<PacketInt>(packet.Length).Read());
			Write(packet.Read());
		}
		
		public TPacket Get<TPacket>() where TPacket: APacket, new() {
			TPacket packet = new TPacket();
			int? fixedSize = packet.GetFixedSize();
			if (fixedSize.HasValue)
				packet.Write(Read(fixedSize.Value));
			else {
				PacketInt i = new PacketInt();
				i.Write(Read(sizeof(int)));
				packet.Write(Read(i.ReadType()));
			}
			return packet;
		}
	}
}