using System;
using UnityEngine;

namespace Network.Packet.PacketType {
	public sealed class PacketVector2: PacketType<Vector2> {
		public override int? GetFixedSize() => 2 * sizeof(float);
		
		public PacketVector2() {}
		public PacketVector2(byte[] bytes): base(bytes) { }
		public PacketVector2(Vector2 data) { WriteType(data); }
		
		public override Vector2 ReadType() {
			byte[] buff = Read();
			return new Vector2 {
				x = BitConverter.ToSingle(buff, 0),
				y = BitConverter.ToSingle(buff, 4)
			};
		}

		public override void WriteType(Vector2 position) {
			const int fSize = sizeof(float);
			byte[] bytes = new byte[2 * fSize];
			Buffer.BlockCopy(BitConverter.GetBytes(position.x), 0, bytes, 0, fSize);
			Buffer.BlockCopy(BitConverter.GetBytes(position.y), 0, bytes, fSize, fSize);
			Write(bytes);
		}
	}
}