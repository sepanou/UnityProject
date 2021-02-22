using System;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Packet.PacketType {
	public sealed class PacketQuaternion: PacketType<Quaternion> {
		public override int? GetFixedSize() => 4 * sizeof(float);
		
		public static PacketQuaternion Make(Quaternion data)
			=> PacketType<Quaternion>.Make<PacketQuaternion>(data);
		
		public override Quaternion ReadType() {
			byte[] buff = Read();
			return new Quaternion {
				x = BitConverter.ToSingle(buff, 0),
				y = BitConverter.ToSingle(buff, 4),
				z = BitConverter.ToSingle(buff, 8),
				w = BitConverter.ToSingle(buff, 12)
			};
		}

		public override void WriteType(Quaternion position) {
			const int fSize = sizeof(float);
			byte[] bytes = new byte[4 * fSize];
			Buffer.BlockCopy(BitConverter.GetBytes(position.x), 0, bytes, 0, fSize);
			Buffer.BlockCopy(BitConverter.GetBytes(position.y), 0, bytes, fSize, fSize);
			Buffer.BlockCopy(BitConverter.GetBytes(position.z), 0, bytes, 2 * fSize, fSize);
			Buffer.BlockCopy(BitConverter.GetBytes(position.w), 0, bytes, 3 * fSize, fSize);
			Write(bytes);
		}
	}
}