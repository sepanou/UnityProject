using System;

namespace Network.Packet.PacketType {
	public sealed class PacketFloat: PacketType<float> {
		private static readonly int? FixedSize = sizeof(float);
		public override int? GetFixedSize() => FixedSize;
		
		public static PacketFloat Make(float data)
			=> PacketType<float>.Make<PacketFloat>(data);
		
		public override float ReadType()
			=>  BitConverter.ToSingle(Read(), 0);

		public override void WriteType(float data)
			=> Write(BitConverter.GetBytes(data));
	}
}