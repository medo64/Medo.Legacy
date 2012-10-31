using Medo.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class KeyValuePacketTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void KeyValuePacket_ToArray_NullKeyValue() {
            KeyValuePacket kvp = new KeyValuePacket();
            byte[] result = kvp.ToArray();
            string resultString = System.BitConverter.ToString(result);

            Assert.AreEqual("4B-56-50-01-FF-FF-FF-FF-FF-FF-FF-FF-4B-56-50-01", resultString);
        }

        [TestMethod()]
        public void KeyValuePacket_ToArray_NullKey() {
            KeyValuePacket kvp = new KeyValuePacket(null, new byte[] { });
            byte[] result = kvp.ToArray();
            string resultString = System.BitConverter.ToString(result);

            Assert.AreEqual("4B-56-50-01-FF-FF-FF-FF-00-00-00-00-B4-A9-AF-FE", resultString);
        }

        [TestMethod()]
        public void KeyValuePacket_ToArray_NullValue() {
            KeyValuePacket kvp = new KeyValuePacket("", null);
            byte[] result = kvp.ToArray();
            string resultString = System.BitConverter.ToString(result);

            Assert.AreEqual("4B-56-50-01-00-00-00-00-FF-FF-FF-FF-B4-A9-AF-FE", resultString);
        }

        [TestMethod()]
        public void KeyValuePacket_ToArray() {
            KeyValuePacket kvp = new KeyValuePacket("XXX", new byte[] { 0, 1, 2, 3 });
            byte[] result = kvp.ToArray();
            string resultString = System.BitConverter.ToString(result);

            Assert.AreEqual("4B-56-50-01-00-00-00-03-00-00-00-04-4B-52-53-01-58-58-58-00-01-02-03", resultString);
        }


        [TestMethod()]
        [ExpectedException(typeof(System.FormatException))]
        public void KeyValuePacket_Parse_CrcError() {
            byte[] input = new byte[] { 0x4B, 0x56, 0x50, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x4B, 0x56, 0x50, 0x02 };

            KeyValuePacket kvp = KeyValuePacket.Parse(input);
            Assert.AreEqual(kvp.Key, null);
            Assert.AreEqual(kvp.GetValue(), null);
        }

        [TestMethod()]
        public void KeyValuePacket_Parse_NullKeyValue() {
            byte[] input = new byte[] { 0x4B, 0x56, 0x50, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x4B, 0x56, 0x50, 0x01 };

            KeyValuePacket kvp = KeyValuePacket.Parse(input);
            Assert.AreEqual(kvp.Key, null);
            Assert.AreEqual(kvp.GetValue(), null);
        }

        [TestMethod()]
        public void KeyValuePacket_Parse_NullKey() {
            byte[] input = new byte[] { 0x4B, 0x56, 0x50, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xB4, 0xA9, 0xAF, 0xFE };

            KeyValuePacket kvp = KeyValuePacket.Parse(input);
            Assert.AreEqual(kvp.Key, null);
            Assert.AreEqual(kvp.GetValue().Length, 0);
        }

        [TestMethod()]
        public void KeyValuePacket_Parse_NullValue() {
            byte[] input = new byte[] { 0x4B, 0x56, 0x50, 0x01, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xB4, 0xA9, 0xAF, 0xFE };

            KeyValuePacket kvp = KeyValuePacket.Parse(input);
            Assert.AreEqual(kvp.Key.Length, 0);
            Assert.AreEqual(kvp.GetValue(), null);
        }

        [TestMethod()]
        public void KeyValuePacket_Parse() {
            byte[] input = new byte[] { 0x4B, 0x56, 0x50, 0x01, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x4B, 0x52, 0x53, 0x01, 0x58, 0x58, 0x58, 0x00, 0x01, 0x02, 0x03 };

            KeyValuePacket kvp = KeyValuePacket.Parse(input);
            Assert.AreEqual(kvp.Key, "XXX");
            Assert.AreEqual(kvp.GetValue()[0], 0);
            Assert.AreEqual(kvp.GetValue()[1], 1);
            Assert.AreEqual(kvp.GetValue()[2], 2);
            Assert.AreEqual(kvp.GetValue()[3], 3);
        }

    }
}
