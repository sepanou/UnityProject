namespace Network.Packet.PacketType {
	public abstract class PacketType<T>: APacket {
		public abstract void Read(out T data);
		public abstract void Write(T data);
	}
}