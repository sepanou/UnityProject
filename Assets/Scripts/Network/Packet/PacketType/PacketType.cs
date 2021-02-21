namespace Network.Packet.PacketType {
	public abstract class PacketType<T>: APacket {
		public static TPacket Make<TPacket>(T data) where TPacket: PacketType<T>, new() {
			TPacket packet = new TPacket();
			packet.WriteType(data);
			return packet;
		}
		
		public abstract T ReadType();
		public abstract void WriteType(T data);
	}
}
