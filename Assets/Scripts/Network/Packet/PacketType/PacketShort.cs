using System;

namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for <see cref="short"/>
	/// </summary>
	public sealed class PacketShort: PacketType<short> {
		/// <summary> a short has a fixed size of 2 bytes </summary>
		/// <returns> fixed size </returns>
		public override ushort? GetFixedSize() => sizeof(short);
		
		/// <summary> creates empty packet </summary>
		public PacketShort() {}
		/// <summary> creates packet from raw data </summary>
		public PacketShort(byte[] bytes): base(bytes) { }
		/// <summary> creates packet from a short </summary>
		public PacketShort(short data) { WriteType(data); }

		/// <summary> reads a short from the packet </summary>
		/// <returns> read data </returns>
		public override short ReadType()
			=> BitConverter.ToInt16(Read(), 0);

		/// <summary> writes a short to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(short data)
			=> Write(BitConverter.GetBytes(data));
	}
}