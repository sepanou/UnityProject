using System;
using UnityEngine;

namespace Network.Packet.PacketType {
	public sealed class PacketVector3: PacketType<Vector3> {
		public override Vector3 ReadType() {
			byte[] buff = Read();
			return new Vector3 {
				x = BitConverter.ToSingle(buff, 0),
				y = BitConverter.ToSingle(buff, 4),
				z = BitConverter.ToSingle(buff, 8)
			};
		}

		public override void WriteType(Vector3 position) {
			const int fSize = sizeof(float);
			byte[] bytes = new byte[3 * fSize];
			Buffer.BlockCopy(BitConverter.GetBytes(position.x), 0, bytes, 0, fSize);
			Buffer.BlockCopy(BitConverter.GetBytes(position.y), 0, bytes, fSize, fSize);
			Buffer.BlockCopy(BitConverter.GetBytes(position.z), 0, bytes, 2 * fSize, fSize);
			Write(bytes);
		}
	}
}