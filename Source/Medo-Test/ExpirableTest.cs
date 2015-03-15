using Medo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace Test {

    [TestClass()]
    public class ExpirableTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void Expirable_Basic() {
            var v = DateTime.UtcNow;
            var x = new Expirable<DateTime>(new TimeSpan(0, 0, 0, 0, 10), v);

            Assert.AreEqual(true, x.HasValue);
            Assert.AreEqual(v, x.Value);

            Thread.Sleep(10);

            Assert.AreEqual(false, x.HasValue);
            Assert.AreEqual(DateTime.MinValue, x);
            Assert.AreEqual(DateTime.MinValue.ToString(), x.ToString());
        }

        [TestMethod()]
        public void Expirable_Defaults() {
            var v = DateTime.UtcNow;
            var x = new Expirable<DateTime>(new TimeSpan(0, 0, 0, 0, 10), v, DateTime.MaxValue);

            Assert.AreEqual(true, x.HasValue);
            Assert.AreEqual(v, x.Value);

            Thread.Sleep(10);

            Assert.AreEqual(false, x.HasValue);
            Assert.AreEqual(DateTime.MaxValue, x);
        }

        [TestMethod()]
        public void Expirable_EqualsStruct() {
            var v = DateTime.UtcNow;
            var x = new Expirable<DateTime>(new TimeSpan(0, 0, 0, 0, 10), v);

            Assert.IsTrue(x.Equals(x));
            Assert.IsTrue(x.Equals(v));
            Assert.IsTrue(x.Equals(new Expirable<DateTime>(3, v)));
            Assert.AreEqual(v.ToString(), x.ToString());

            Assert.IsFalse(x.Equals(v.AddTicks(1)));
            Assert.IsFalse(x.Equals(new Expirable<DateTime>(3, v.AddTicks(1))));

            Assert.IsFalse(x.Equals(null));

            x.Expire();

            Assert.IsTrue(x.Equals(DateTime.MinValue));
            Assert.AreEqual(DateTime.MinValue.ToString(), x.ToString());
        }

        [TestMethod()]
        public void Expirable_EqualsClass() {
            var v = "Test";
            var x = new Expirable<string>(new TimeSpan(0, 0, 0, 0, 10), v);

            Assert.IsTrue(x.Equals(x));
            Assert.IsTrue(x.Equals(v));
            Assert.IsTrue(x.Equals(new Expirable<string>(3, "Test")));
            Assert.AreEqual(v.ToString(), x.ToString());

            Assert.IsFalse(x.Equals("Test2"));
            Assert.IsFalse(x.Equals(new Expirable<string>(3, "Test2")));

            Assert.IsFalse(x.Equals(null));

            x.Expire();

            Assert.IsTrue(x.Equals(null));
            Assert.AreEqual("", x.ToString());
        }

    }
}
