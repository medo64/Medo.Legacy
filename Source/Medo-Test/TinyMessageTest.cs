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
            Assert.AreEqual("Value1Text", actual["Key1Text"]);
            Assert.AreEqual("Value2Text", actual["Key2Text"]);
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
            Assert.AreEqual("A\n\rB", actual["Key1"]);
            Assert.AreEqual("\0", actual["Key2"]);
            Assert.AreEqual("\\", actual["Key3"]);
        }


        [TestMethod()]
        public void Test_TinyPacket_Encode_NullItem_01() {
            var target = new TinyPacket("Example", "Test");
            target["A"] = "1";
            target["B"] = null;
            target["C"] = "2";
            Assert.AreEqual(@"Tiny Example Test {""A"":""1"",""B"":null,""C"":""2""}", UTF8Encoding.UTF8.GetString(target.GetBytes()));
        }

        [TestMethod()]
        public void Test_TinyPacket_Decode_NullItem_01A() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test {""A"":""1"",""B"":null,""C"":""2""}"));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
            Assert.AreEqual("1", actual["A"]);
            Assert.AreEqual(null, actual["B"]);
            Assert.AreEqual("2", actual["C"]);
        }

        [TestMethod()]
        public void Test_TinyPacket_Decode_NullItem_01B() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test { ""A"":""1"", ""B"" : null, ""C"":""2"" }"));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
            Assert.AreEqual("1", actual["A"]);
            Assert.AreEqual(null, actual["B"]);
            Assert.AreEqual("2", actual["C"]);
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
        }

        [TestMethod()]
        public void Test_TinyPacket_Decode_Null() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test  null "));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
        }

        [TestMethod()]
        public void Test_TinyPacket_Decode_MissingData_01() { //it is an error state, but we shall recognize it.
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test "));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
        }

        [TestMethod()]
        public void Test_TinyPacket_Decode_MissingData_02() { //it is an error state, but we shall recognize it.
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test"));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
        }

        [TestMethod()]
        public void Test_TinyPacket_Encode_Indexer() {
            var target = new TinyPacket("Example", "Test");
            target["A"] = "0";
            target["A"] = "1";

            Assert.AreEqual(@"Tiny Example Test {""A"":""1""}", UTF8Encoding.UTF8.GetString(target.GetBytes()));
        }

        [TestMethod()]
        public void Test_TinyPacket_Decode_Indexer() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test {""A"":""0"",""A"":""1"",""B"":""null"",""B"":null}"));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
            Assert.AreEqual("1", actual["A"]);
            Assert.AreEqual(null, actual["B"]);
            Assert.AreEqual(null, actual["C"]);
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
                target["Key1Text"] = "Value1Text";
                target["Key2Text"] = "Value2Text";
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
