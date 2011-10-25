using Medo.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;

namespace Test {

    [TestClass()]
    public class TinyPairTest {

        private TestContext testContextInstance;

        public TestContext TestContext {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }


        [TestMethod()]
        public void Test_TinyPairPacket_Encode_Default() {
            string product = "Example";
            string operation = "Test";
            var data = new Dictionary<string, string>();
            data.Add("Key1Text", "Value1Text");
            data.Add("Key2Text", "Value2Text");

            var target = new TinyPairPacket(product, operation, data);

            string actual = System.Text.UTF8Encoding.UTF8.GetString(target.GetBytes());
            string expected = @"Tiny Example Test [{""Key"":""Key1Text"",""Value"":""Value1Text""},{""Key"":""Key2Text"",""Value"":""Value2Text""}]";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Test_TinyPairPacket_Decode_Default() {
            string product = "Example";
            string operation = "Test";
            var data = new Dictionary<string, string>();
            data.Add("Key1Text", "Value1Text");
            data.Add("Key2Text", "Value2Text");

            TinyPairPacket actual = TinyPairPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test [{""Key"":""Key1Text"",""Value"":""Value1Text""},{""Key"":""Key2Text"",""Value"":""Value2Text""}]"));

            Assert.AreEqual(product, actual.Product);
            Assert.AreEqual(operation, actual.Operation);
            Assert.AreEqual(data.Count, actual.Data.Count);
            foreach (var key in data.Keys) {
                if (actual.Data.ContainsKey(key)) {
                    Assert.AreEqual(data[key], actual.Data[key]);
                } else {
                    Assert.Fail("Content mismatch.");
                }
            }
        }

        [TestMethod()]
        public void Test_TinyPairPacket_Encode_DataEmpty() {
            string product = "Example";
            string operation = "Test";
            var data = new Dictionary<string, string>();

            var target = new TinyPairPacket(product, operation, data);

            string actual = System.Text.UTF8Encoding.UTF8.GetString(target.GetBytes());
            string expected = @"Tiny Example Test []";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Test_TinyPairPacket_Decode_DataEmpty() {
            string product = "Example";
            string operation = "Test";

            TinyPairPacket actual = TinyPairPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test []"));

            Assert.AreEqual(product, actual.Product);
            Assert.AreEqual(operation, actual.Operation);
            Assert.AreEqual(0, actual.Data.Count);
        }

    }
}
