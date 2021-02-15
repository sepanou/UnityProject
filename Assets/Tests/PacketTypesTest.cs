using System.Collections.Generic;
using Network.Packet.PacketType;
using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace Tests {
	public class PacketTypesTest {
		private static readonly Random Random = new Random();

		private static bool PacketTypeTest<TP, T>(T ta) where TP: PacketType<T>, new() {
			TP p = new TP();
			p.Write(ta);
			p.Read(out T tb);
			return EqualityComparer<T>.Default.Equals(ta, tb);
		}

		[Test] public void Int32Packets() => Assert.IsTrue(PacketTypeTest<PacketInt32, int>(Random.Next()));
		[Test] public void Int64Packets() => Assert.IsTrue(PacketTypeTest<PacketInt64, long>(Random.Next()));
		[Test] public void FloatPackets() => Assert.IsTrue(PacketTypeTest<PacketFloat, float>(Random.Next()));
		[Test] public void DoublePackets() => Assert.IsTrue(PacketTypeTest<PacketDouble, double>(Random.Next()));
		[Test] public void StringPackets() => Assert.IsTrue(PacketTypeTest<PacketString, string>("Tester c'est doubter"));
		[Test] public void Vector3Packets() => Assert.IsTrue(PacketTypeTest<PacketPosition, Vector3>(new Vector3 {
			x = Random.Next(),
			y = Random.Next(),
			z = Random.Next()
		}));
		[Test] public void QuaternionPackets() => Assert.IsTrue(PacketTypeTest<PacketRotation, Quaternion>(new Quaternion {
			x = Random.Next(),
			y = Random.Next(),
			z = Random.Next(),
			w = Random.Next()
		}));
	}
}