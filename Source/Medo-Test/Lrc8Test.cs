using Medo.Security.Checksum;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class Lrc8Test {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void Lrc8_Eltra() {
            string expected = "1A";
            Lrc8 actualCrc = Lrc8.GetEltra();
            actualCrc.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", true);
            string actual = actualCrc.Digest.ToString("X2");
            Assert.AreEqual(expected, actual);
        }

    }
}
