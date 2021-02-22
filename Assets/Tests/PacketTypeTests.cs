using System.Collections.Generic;

using NUnit.Framework;
using UnityEngine;

using Network.Packet.PacketType;

namespace Tests {
	public class PacketTypeTests {
		private static readonly System.Random Random = new System.Random();

		private static bool PacketTypeTest<TP, T>(T ta) where TP: PacketType<T>, new() {
			TP p = new TP();
			p.WriteType(ta);
			return EqualityComparer<T>.Default.Equals(ta, p.ReadType());
		}

		private static bool PacketTypeTest<TP>(byte[] ta) where TP: PacketType<byte[]>, new() {
			TP p = new TP();
			p.WriteType(ta);
			byte[] tb = p.ReadType();
			bool same = ta.Length == tb.Length;
			for (int i = 0; same && i < ta.Length; ++i)
				same = ta[i] == tb[i];
			return same;
		}

		[Test] public void BytePackets() => Assert.IsTrue(PacketTypeTest<PacketByte, byte>((byte)Random.Next()));
		[Test] public void BytesPackets() => Assert.IsTrue(PacketTypeTest<PacketBytes>(new [] {
			(byte)Random.Next(),
			(byte)Random.Next(),
			(byte)Random.Next()
		}));
		[Test] public void IntPackets() => Assert.IsTrue(PacketTypeTest<PacketInt, int>(Random.Next()));
		[Test] public void LongPackets() => Assert.IsTrue(PacketTypeTest<PacketLong, long>(Random.Next()));
		[Test] public void FloatPackets() => Assert.IsTrue(PacketTypeTest<PacketFloat, float>(Random.Next()));
		[Test] public void DoublePackets() => Assert.IsTrue(PacketTypeTest<PacketDouble, double>(Random.Next()));
		[Test] public void StringPackets() => Assert.IsTrue(PacketTypeTest<PacketString, string>("Tester c'est doubter"));
		[Test] public void Vector2Packets() => Assert.IsTrue(PacketTypeTest<PacketVector2, Vector2>(new Vector2 {
			x = Random.Next(),
			y = Random.Next()
		}));
		[Test] public void Vector3Packets() => Assert.IsTrue(PacketTypeTest<PacketVector3, Vector3>(new Vector3 {
			x = Random.Next(),
			y = Random.Next(),
			z = Random.Next()
		}));
		[Test] public void QuaternionPackets() => Assert.IsTrue(PacketTypeTest<PacketQuaternion, Quaternion>(new Quaternion {
			x = Random.Next(),
			y = Random.Next(),
			z = Random.Next(),
			w = Random.Next()
		}));
	}
}