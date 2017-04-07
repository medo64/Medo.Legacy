using Medo.Security.Checksum;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class Iso7064Test {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void Iso7064_01() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("0");
            char actual = actualCrc.Digest;
            Assert.AreEqual('2', actual);
        }

        [TestMethod()]
        public void Iso7064_02() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("1");
            char actual = actualCrc.Digest;
            Assert.AreEqual('9', actual);
        }

        [TestMethod()]
        public void Iso7064_03() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("6");
            char actual = actualCrc.Digest;
            Assert.AreEqual('0', actual);
        }

        [TestMethod()]
        public void Iso7064_04() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("9");
            char actual = actualCrc.Digest;
            Assert.AreEqual('4', actual);
        }

        [TestMethod()]
        public void Iso7064_05() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("0823");
            char actual = actualCrc.Digest;
            Assert.AreEqual('5', actual);
        }

        [TestMethod()]
        public void Iso7064_06() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("276616973212561");
            char actual = actualCrc.Digest;
            Assert.AreEqual('5', actual);
        }

        [TestMethod()]
        public void Iso7064_07() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("65");
            char actual = actualCrc.Digest;
            Assert.AreEqual('0', actual);
        }

        [TestMethod()]
        public void Iso7064_08() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("56");
            char actual = actualCrc.Digest;
            Assert.AreEqual('0', actual);
        }

        [TestMethod()]
        public void Iso7064_09() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("732");
            char actual = actualCrc.Digest;
            Assert.AreEqual('5', actual);
        }

        [TestMethod()]
        public void Iso7064_10() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("723");
            char actual = actualCrc.Digest;
            Assert.AreEqual('5', actual);
        }

        [TestMethod()]
        public void Iso7064_11() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("8373426074");
            char actual = actualCrc.Digest;
            Assert.AreEqual('9', actual);
        }

        [TestMethod()]
        public void Iso7064_12() {
            Iso7064 actualCrc = new Iso7064();
            actualCrc.Append("4428922675");
            char actual = actualCrc.Digest;
            Assert.AreEqual('7', actual);
        }

    }
}
