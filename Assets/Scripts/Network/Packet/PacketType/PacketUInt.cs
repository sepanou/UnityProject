using System;

namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for <see cref="uint"/>
	/// </summary>
	public sealed class PacketUInt: PacketType<uint> {
		/// <summary> a uint has a fixed size of 4 bytes </summary>
		/// <returns> fixed size </returns>
		public override ushort? GetFixedSize() => sizeof(uint);
		
		/// <summary> creates empty packet </summary>
		public PacketUInt() {}
		/// <summary> creates packet from raw data </summary>
		public PacketUInt(byte[] bytes): base(bytes) { }
		/// <summary> creates packet from a uint </summary>
		public PacketUInt(uint data) { WriteType(data); }
		
		/// <summary> reads a uint from the packet </summary>
		/// <returns> read data </returns>
		public override uint ReadType()
			=> BitConverter.ToUInt32(Read(), 0);

		/// <summary> writes a uint to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(uint data)
			=> Write(BitConverter.GetBytes(data));
	}
}