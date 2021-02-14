using System;
using Network.Packet.PacketType;
using NUnit.Framework;

namespace Tests {
    public class PacketTypesTest {
        private Random _random;
        
        [SetUp]
        public void Setup() {
            _random = new Random();
        }
        
        [Test]
        public void Int32Packets() {
            int testInt = _random.Next();
            PacketInt32 packetInt32 = new PacketInt32(testInt);
            packetInt32.Read(out int returnInt);
            Assert.AreEqual(testInt, returnInt);
        }
        
        [Test]
        public void Int64Packets() {
            long testInt = _random.Next();
            PacketInt64 packetInt32 = new PacketInt64(testInt);
            packetInt32.Read(out long returnInt);
            Assert.AreEqual(testInt, returnInt);
        }
        
        [Test]
        public void FloatPackets() {
            float testFloat = _random.Next();
            PacketFloat packetFloat = new PacketFloat(testFloat);
            packetFloat.Read(out float returnFloat);
            Assert.AreEqual(testFloat, returnFloat);
        }
        
        [Test]
        public void DoublePackets() {
            double testDouble = _random.Next();
            PacketDouble packetFloat = new PacketDouble(testDouble);
            packetFloat.Read(out double returnDouble);
            Assert.AreEqual(testDouble, returnDouble);
        }
        
        [Test]
        public void StringPackets() {
            const string testString = "Tester c'est douter";
            PacketString packetString = new PacketString(testString);
            packetString.Read(out string returnString);
            Assert.AreEqual(testString, returnString);
        }
    }
}