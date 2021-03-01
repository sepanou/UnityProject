using PacketUShort = Network.Packet.PacketType.PacketUShort;

namespace Network.Packet.Packets {
	/// <summary>
	/// packets that contain other packets
	/// </summary>
	public class Packets: APacket {
		/// <summary> creates empty packet </summary>
		public Packets() {}
		/// <summary> creates packet from raw data </summary>
		public Packets(byte[] bytes): base(bytes) { }
		
		/// <summary> add a sub-packet to the packets </summary>
		/// <param name="packet"> sub-packet to add </param>
		public void Add(APacket packet) {
			if (!packet.GetFixedSize().HasValue)
				Write(new PacketUShort(packet.Length).Read());
			Write(packet.Read());
		}
		
		/// <summary> removes and returns a sub-packet </summary>
		/// <typeparam name="TPacket"> type of the sub-packet </typeparam>
		/// <returns> sub-packet </returns>
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