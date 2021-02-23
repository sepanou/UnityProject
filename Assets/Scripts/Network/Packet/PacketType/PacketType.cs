namespace Network.Packet.PacketType {
	public abstract class PacketType<T>: APacket {
		public PacketType() {}
		public PacketType(byte[] bytes): base(bytes) { }
		
		public abstract T ReadType();
		public abstract void WriteType(T data);
	}
}
