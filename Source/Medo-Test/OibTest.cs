using Medo.Localization.Croatia;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class OibTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void Oib_1() {
            Oib target = new Oib("83734260749");
            Assert.AreEqual(true, target.IsValid);
        }

        [TestMethod()]
        public void Oib_2() {
            Oib target = new Oib("44289226757");
            Assert.AreEqual(true, target.IsValid);
        }

        [TestMethod()]
        public void Oib_3() {
            Oib target = new Oib("11111111111");
            Assert.AreEqual(false, target.IsValid);
        }

        [TestMethod()]
        public void Oib_4() {
            Oib target = new Oib("7235");
            Assert.AreEqual(false, target.IsValid);
        }

    }
}
