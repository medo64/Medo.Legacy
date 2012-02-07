using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Medo.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class TinyMessageTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void Test_TinyPacket_Encode_01() {
            var target = new TinyPacket("Example", "Test");
            target["Key1Text"] = "Value1Text";
            target["Key2Text"] = "Value2Text";
            Assert.AreEqual(@"Tiny Example Test {""Key1Text"":""Value1Text"",""Key2Text"":""Value2Text""}", UTF8Encoding.UTF8.GetString(target.GetBytes()));
        }

        [TestMethod()]
        public void Test_TinyPacket_Decode_01() {
            var data = new Dictionary<string, string>();
            data.Add("Key1Text", "Value1Text");
            data.Add("Key2Text", "Value2Text");

            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test {""Key1Text"":""Value1Text"",""Key2Text"":""Value2Text""}  "));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
            Assert.AreEqual(data.Count, actual.Items.Count);
            foreach (var key in data.Keys) {
                if (actual.Items.ContainsKey(key)) {
                    Assert.AreEqual(data[key], actual.Items[key]);
                } else {
                    Assert.Fail("Content mismatch.");
                }
            }
        }

        [TestMethod()]
        public void Test_TinyPacket_Encode_02() {
            var target = new TinyPacket("Example", "Test");
            target["Key1"] = "A\n\rB";
            target["Key2"] = "\0";
            target["Key3"] = "\\";
            Assert.AreEqual(@"Tiny Example Test {""Key1"":""A\n\rB"",""Key2"":""\u0000"",""Key3"":""\\""}", UTF8Encoding.UTF8.GetString(target.GetBytes()));
        }

        [TestMethod()]
        public void Test_TinyPacket_Decode_02() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test {""Key1"":""A\n\rB"",""Key2"":""\u0000"",""Key3"":""\\""}"));
            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
            Assert.AreEqual("A\n\rB", actual.Items["Key1"]);
            Assert.AreEqual("\0", actual.Items["Key2"]);
            Assert.AreEqual("\\", actual.Items["Key3"]);
        }


        [TestMethod()]
        public void Test_TinyPacket_Encode_Empty() {
            var target = new TinyPacket("Example", "Test");
            Assert.AreEqual(@"Tiny Example Test {}", UTF8Encoding.UTF8.GetString(target.GetBytes()));
        }

        [TestMethod()]
        public void Test_TinyPacket_Decode_Empty() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test  {} "));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
            Assert.AreEqual(0, actual.Items.Count);
        }

        [TestMethod()]
        public void Test_TinyPacket_Decode_Null() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test  null "));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
            Assert.AreEqual(0, actual.Items.Count);
        }

        [TestMethod()]
        public void Test_TinyPacket_Decode_MissingData_01() { //it is an error state, but we shall recognize it.
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test "));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
            Assert.AreEqual(0, actual.Items.Count);
        }

        [TestMethod()]
        public void Test_TinyPacket_Decode_MissingData_02() { //it is an error state, but we shall recognize it.
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test"));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
            Assert.AreEqual(0, actual.Items.Count);
        }

        [TestMethod()]
        [ExpectedException(typeof(FormatException))]
        public void Test_TinyPacket_Decode_Error_01() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example "));
        }

        [TestMethod()]
        [ExpectedException(typeof(FormatException))]
        public void Test_TinyPacket_Decode_Error_02() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny "));
        }

        [TestMethod()]
        [ExpectedException(typeof(FormatException))]
        public void Test_TinyPacket_Decode_Error_03() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@""));
        }


        [TestMethod()]
        public void Test_TinyPacket_EncodeDecode_SpeedTest() {
            var data = new Dictionary<string, string>();
            data.Add("Key1Text", "Value1Text");
            data.Add("Key2Text", "Value2Text");

            TinyPacket target = null;
            byte[] bytes = null;

            var n = 100000;
            var swEncode = new Stopwatch();
            swEncode.Start();
            for (int i = 0; i < n; i++) {
                target = new TinyPacket("Example", "Test");
                target.Items.Add("Key1Text", "Value1Text");
                target.Items.Add("Key2Text", "Value2Text");
                bytes = target.GetBytes();
            }
            swEncode.Stop();

            var swDecode = new Stopwatch();
            swDecode.Start();
            for (int i = 0; i < n; i++) {
                var target2 = TinyPacket.Parse(bytes);
            }
            swDecode.Stop();

            //Assert.Inconclusive(string.Format("TinyPair (encode/decode): {0} + {1} = {2} ms", swEncode.ElapsedMilliseconds, swDecode.ElapsedMilliseconds, swEncode.ElapsedMilliseconds + swDecode.ElapsedMilliseconds));
        }

    }
}
