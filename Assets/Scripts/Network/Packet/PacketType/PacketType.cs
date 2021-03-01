namespace Network.Packet.PacketType {
	/// <summary>
	/// abstract class from specific types packets
	/// </summary>
	/// <typeparam name="T"> specific type of the packet </typeparam>
	public abstract class PacketType<T>: APacket {
		/// <summary> creates empty packet </summary>
		protected PacketType() {}
		/// <summary> creates packet from raw data </summary>
		protected PacketType(byte[] bytes): base(bytes) { }
		
		/// <summary> reads data from the packet </summary>
		/// <returns> data of specific type </returns>
		public abstract T ReadType();
		/// <summary> writes data to the packet </summary>
		/// <param name="data"> data of specific type </param>
		public abstract void WriteType(T data);
	}
}
