using Medo.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace Test {

    [TestClass()]
    public class TinyPairTest {

        private TestContext testContextInstance;

        public TestContext TestContext {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }


        [TestMethod()]
        public void Test_TinyPairPacket_Encode_Default_AsArray() {
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
        public void Test_TinyPairPacket_Encode_Default_AsObject() {
            string product = "Example";
            string operation = "Test";
            var data = new Dictionary<string, string>();
            data.Add("Key1Text", "Value1Text");
            data.Add("Key2Text", "Value2Text");

            var target = new TinyPairPacket(product, operation, data);
            target.UseObjectEncoding = true;

            string actual = System.Text.UTF8Encoding.UTF8.GetString(target.GetBytes());
            string expected = @"Tiny Example Test {""Key1Text"":""Value1Text"",""Key2Text"":""Value2Text""}";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Test_TinyPairPacket_Decode_Default_AsArray() {
            string product = "Example";
            string operation = "Test";
            var data = new Dictionary<string, string>();
            data.Add("Key1Text", "Value1Text");
            data.Add("Key2Text", "Value2Text");

            TinyPairPacket actual = TinyPairPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test [{""Key"":""Key1Text"",""Value"":""Value1Text""},{""Key"":""Key2Text"",""Value"":""Value2Text""}]  "));

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
        public void Test_TinyPairPacket_Decode_Default_AsObject() {
            string product = "Example";
            string operation = "Test";
            var data = new Dictionary<string, string>();
            data.Add("Key1Text", "Value1Text");
            data.Add("Key2Text", "Value2Text");

            TinyPairPacket actual = TinyPairPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test {""Key1Text"":""Value1Text"",""Key2Text"":""Value2Text""}  "));

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
        public void Test_TinyPairPacket_Encode_DataEmpty_AsArray() {
            string product = "Example";
            string operation = "Test";
            var data = new Dictionary<string, string>();

            var target = new TinyPairPacket(product, operation, data);

            string actual = System.Text.UTF8Encoding.UTF8.GetString(target.GetBytes());
            string expected = @"Tiny Example Test []";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Test_TinyPairPacket_Encode_DataEmpty_AsObject() {
            string product = "Example";
            string operation = "Test";
            var data = new Dictionary<string, string>();

            var target = new TinyPairPacket(product, operation, data);
            target.UseObjectEncoding = true;

            string actual = System.Text.UTF8Encoding.UTF8.GetString(target.GetBytes());
            string expected = @"Tiny Example Test {}";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Test_TinyPairPacket_Encode_DataNull_AsArray() { //encodes it empty because of compatibility
            string product = "Example";
            string operation = "Test";

            var target = new TinyPairPacket(product, operation, null);

            string actual = System.Text.UTF8Encoding.UTF8.GetString(target.GetBytes());
            string expected = @"Tiny Example Test []";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Test_TinyPairPacket_Encode_DataNull_AsObject() {
            string product = "Example";
            string operation = "Test";

            var target = new TinyPairPacket(product, operation, null);
            target.UseObjectEncoding = true;

            string actual = System.Text.UTF8Encoding.UTF8.GetString(target.GetBytes());
            string expected = @"Tiny Example Test null";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Test_TinyPairPacket_Decode_DataEmpty_AsArray() {
            string product = "Example";
            string operation = "Test";

            TinyPairPacket actual = TinyPairPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test  [] "));

            Assert.AreEqual(product, actual.Product);
            Assert.AreEqual(operation, actual.Operation);
            Assert.AreEqual(0, actual.Data.Count);
        }

        [TestMethod()]
        public void Test_TinyPairPacket_Decode_DataEmpty_AsObject() {
            string product = "Example";
            string operation = "Test";

            TinyPairPacket actual = TinyPairPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test  {} "));

            Assert.AreEqual(product, actual.Product);
            Assert.AreEqual(operation, actual.Operation);
            Assert.AreEqual(0, actual.Data.Count);
        }

        [TestMethod()]
        public void Test_TinyPairPacket_Decode_DataNull() {
            string product = "Example";
            string operation = "Test";

            TinyPairPacket actual = TinyPairPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test  null "));

            Assert.AreEqual(product, actual.Product);
            Assert.AreEqual(operation, actual.Operation);
            Assert.AreEqual(0, actual.Data.Count);
        }

        [TestMethod()]
        public void Test_TinyPairPacket_Decode_DataMissing1() { //it is an error state, but we shall recognize it.
            string product = "Example";
            string operation = "Test";

            TinyPairPacket actual = TinyPairPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test "));

            Assert.AreEqual(product, actual.Product);
            Assert.AreEqual(operation, actual.Operation);
            Assert.AreEqual(0, actual.Data.Count);
        }

        [TestMethod()]
        public void Test_TinyPairPacket_Decode_DataMissing2() { //it is an error state, but we shall recognize it.
            string product = "Example";
            string operation = "Test";

            TinyPairPacket actual = TinyPairPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test"));

            Assert.AreEqual(product, actual.Product);
            Assert.AreEqual(operation, actual.Operation);
            Assert.AreEqual(0, actual.Data.Count);
        }

        [TestMethod()]
        [ExpectedException(typeof(System.FormatException))]
        public void Test_TinyPairPacket_Decode_ErrorCannotParsePacket1() {
            TinyPairPacket actual = TinyPairPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example "));
        }

        [TestMethod()]
        [ExpectedException(typeof(System.FormatException))]
        public void Test_TinyPairPacket_Decode_ErrorCannotParsePacket2() {
            TinyPairPacket actual = TinyPairPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny "));
        }

        [TestMethod()]
        [ExpectedException(typeof(System.FormatException))]
        public void Test_TinyPairPacket_Decode_ErrorCannotParsePacket3() {
            TinyPairPacket actual = TinyPairPacket.Parse(UTF8Encoding.UTF8.GetBytes(@""));
        }

        [TestMethod()]
        public void Test_TinyPairPacket_EncodeDecode_SpeedTest() {
            string product = "Example";
            string operation = "Test";
            var data = new Dictionary<string, string>();
            data.Add("Key1Text", "Value1Text");
            data.Add("Key2Text", "Value2Text");

            TinyPairPacket target = null;
            byte[] bytes = null;

            var n = 100000;
            var swEncode = new Stopwatch();
            swEncode.Start();
            for (int i = 0; i < n; i++) {
                target = new TinyPairPacket(product, operation, data);
                target.UseObjectEncoding = true;
                bytes = target.GetBytes();
            }
            swEncode.Stop();

            var swDecode = new Stopwatch();
            swDecode.Start();
            for (int i = 0; i < n; i++) {
                var target2 = TinyPairPacket.Parse(bytes);
            }
            swDecode.Stop();

            //Assert.Inconclusive(string.Format("TinyPair (encode/decode): {0} + {1} = {2} ms", swEncode.ElapsedMilliseconds, swDecode.ElapsedMilliseconds, swEncode.ElapsedMilliseconds + swDecode.ElapsedMilliseconds));
        }

    }
}
