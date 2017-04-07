using Medo.Security.Checksum;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class Crc8Test {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void Crc8_Dallas() {
            string expected = "80";
            Crc8 actualCrc = Crc8.GetDallas();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X2");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Crc8_Maxim() {
            string expected = "80";
            Crc8 actualCrc = Crc8.GetMaxim();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X2");
            Assert.AreEqual(expected, actual);
        }

    }
}
