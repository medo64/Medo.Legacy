using Medo.Security.Checksum;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class Crc16Test {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void Crc16_Zmodem() {
            string expected = "5E1B";
            Crc16 actualCrc = Crc16.GetZmodem();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X4");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Crc16_Xmodem() {
            string expected = "16A3";
            Crc16 actualCrc = Crc16.GetXmodem();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X4");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Crc16_X25() {
            string expected = "CB47";
            Crc16 actualCrc = Crc16.GetX25();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X4");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Crc16_Kermit() {
            string expected = "9839";
            Crc16 actualCrc = Crc16.GetKermit();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X4");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Crc16_Ieee() {
            string expected = "178C";
            Crc16 actualCrc = Crc16.GetIeee();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X4");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Crc16_Ccitt() {
            string expected = "DF2E";
            Crc16 actualCrc = Crc16.GetCcitt();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X4");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Crc16_Arc() {
            string expected = "178C";
            Crc16 actualCrc = Crc16.GetArc();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X4");
            Assert.AreEqual(expected, actual);
        }

    }
}
