using Medo.Localization.Croatia;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class NumberDeclinationTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void NumberDeclination_1() {
            NumberDeclination nd = new NumberDeclination("tim", "tima", "timova");
            Assert.AreEqual("1 tim", nd[1]);
            Assert.AreEqual("2 tima", nd[2]);
            Assert.AreEqual("3 tima", nd[3]);
            Assert.AreEqual("4 tima", nd[4]);
            Assert.AreEqual("5 timova", nd[5]);
            Assert.AreEqual("6 timova", nd[6]);
            Assert.AreEqual("11 timova", nd[11]);
            Assert.AreEqual("21 tim", nd[21]);
            Assert.AreEqual("101 tim", nd[101]);
        }

        [TestMethod()]
        public void NumberDeclination_2() {
            NumberDeclination nd = new NumberDeclination("jabuka", "jabuke", "jabuka");
            Assert.AreEqual("1 jabuka", nd[1]);
            Assert.AreEqual("2 jabuke", nd[2]);
            Assert.AreEqual("3 jabuke", nd[3]);
            Assert.AreEqual("5 jabuka", nd[5]);
            Assert.AreEqual("6 jabuka", nd[6]);
            Assert.AreEqual("11 jabuka", nd[11]);
            Assert.AreEqual("21 jabuka", nd[21]);
            Assert.AreEqual("101 jabuka", nd[101]);
        }

    }
}
