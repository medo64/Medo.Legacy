using Medo.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class WakeOnLanTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void WakeOnLan_1() {
            byte[] target = WakeOnLan.GetPacketBytes("01-02-03-04-05-06", null);
            string result = System.BitConverter.ToString(target);
            Assert.AreEqual("FF-FF-FF-FF-FF-FF-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06", result);
        }

        [TestMethod()]
        public void WakeOnLan_2() {
            byte[] target = WakeOnLan.GetPacketBytes("01:02:03:04:05:06", null);
            string result = System.BitConverter.ToString(target);
            Assert.AreEqual("FF-FF-FF-FF-FF-FF-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06", result);
        }

        [TestMethod()]
        public void WakeOnLan_3() {
            byte[] target = WakeOnLan.GetPacketBytes("01:02:03:04:05:06", "A0:A1:A2:A3:A4:A5");
            string result = System.BitConverter.ToString(target);
            Assert.AreEqual("FF-FF-FF-FF-FF-FF-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-A0-A1-A2-A3-A4-A5", result);
        }

        [TestMethod()]
        public void WakeOnLan_4() {
            byte[] target = WakeOnLan.GetPacketBytes("01-02:03-04:05-06", "A0-A1-A2-A3-A4-A5");
            string result = System.BitConverter.ToString(target);
            Assert.AreEqual("FF-FF-FF-FF-FF-FF-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-01-02-03-04-05-06-A0-A1-A2-A3-A4-A5", result);
        }

    }
}
