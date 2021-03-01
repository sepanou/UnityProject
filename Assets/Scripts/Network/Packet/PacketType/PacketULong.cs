using System;

namespace Network.Packet.PacketType {
	/// <summary>
	/// PacketType for <see cref="ulong"/>
	/// </summary>
	public sealed class PacketULong: PacketType<ulong> {
		/// <summary> a ulong has a fixed size of 8 bytes </summary>
		/// <returns> fixed size </returns>
		public override ushort? GetFixedSize() => sizeof(ulong);
		
		/// <summary> creates empty packet </summary>
		public PacketULong() {}
		/// <summary> creates packet from raw data </summary>
		public PacketULong(byte[] bytes): base(bytes) { }
		/// <summary> creates packet from a ulong </summary>
		public PacketULong(ulong data) { WriteType(data); }
		
		/// <summary> reads a ulong from the packet </summary>
		/// <returns> read data </returns>
		public override ulong ReadType()
			=> BitConverter.ToUInt32(Read(), 0);

		/// <summary> writes a ulong to the packet </summary>
		/// <param name="data"> data to write </param>
		public override void WriteType(ulong data)
			=> Write(BitConverter.GetBytes(data));
	}
}