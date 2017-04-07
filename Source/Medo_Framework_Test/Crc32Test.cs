using Medo.Security.Checksum;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class Crc32Test {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void Crc32_Xfer() {
            string expected = "3A9C355C";
            Crc32 actualCrc = Crc32.GetXfer();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X8");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Crc32_Posix() {
            string expected = "EFC8804E";
            Crc32 actualCrc = Crc32.GetPosix();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X8");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Crc32_Jam() {
            string expected = "E59A841D";
            Crc32 actualCrc = Crc32.GetJam();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X8");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Crc32_Ieee() {
            string expected = "1A657BE2";
            Crc32 actualCrc = Crc32.GetIeee();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X8");
            Assert.AreEqual(expected, actual);
        }

    }
}
