using System;

namespace Network.Packet.PacketType {
	public sealed class PacketLong: PacketType<long> {
		public override long ReadType()
			=> BitConverter.ToInt32(Read(), 0);

		public override void WriteType(long data)
			=> Write(BitConverter.GetBytes(data));
	}
}