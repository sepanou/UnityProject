using System;

namespace Network.Packet.PacketType {
	public sealed class PacketByte: PacketType<byte> {
		public PacketByte() { }

		public PacketByte(byte data) => Write(data);

		public override void Read(out byte data) {
			byte[] input = Read();
			if (input.Length > 0) {
				data = input[0];
			}
			else {
				data = new byte();
			}
		}

		public override void Write(byte data) {
			byte[] array = {data};
			Write(array);
		}
	}
}