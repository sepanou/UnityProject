using System;

namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for <see cref="ushort"/>
	/// </summary>
	public sealed class PacketUShort: PacketType<ushort> {
		/// <summary> a ushort has a fixed size of 2 bytes </summary>
		/// <returns> fixed size </returns>
		public override ushort? GetFixedSize() => sizeof(ushort);
		
		/// <summary> creates empty packet </summary>
		public PacketUShort() {}
		/// <summary> creates packet from raw data </summary>
		public PacketUShort(byte[] bytes): base(bytes) { }
		/// <summary> creates packet from a ushort </summary>
		public PacketUShort(ushort data) { WriteType(data); }

		/// <summary> reads a ushort from the packet </summary>
		/// <returns> read data </returns>
		public override ushort ReadType()
			=> BitConverter.ToUInt16(Read(), 0);

		/// <summary> writes a ushort to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(ushort data)
			=> Write(BitConverter.GetBytes(data));
	}
}