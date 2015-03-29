using Medo.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Test {

    [TestClass()]
    public class TinyPacketTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void TinyPacket_Encode_01() {
            var target = new TinyPacket("Example", "Test");
            target["Key1Text"] = "Value1Text";
            target["Key2Text"] = "Value2Text";

            var actual = UTF8Encoding.UTF8.GetString(target.GetBytes(null, omitIdentifiers: true));

            Assert.AreEqual(@"Tiny Example Test {""Key1Text"":""Value1Text"",""Key2Text"":""Value2Text""}", actual);
        }

        [TestMethod()]
        public void TinyPacket_Decode_01() {
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
        public void TinyPacket_Encode_02() {
            var target = new TinyPacket("Example", "Test");
            target["Key1"] = "A\n\rB";
            target["Key2"] = "\0";
            target["Key3"] = "\\";

            var actual = UTF8Encoding.UTF8.GetString(target.GetBytes(null, omitIdentifiers: true));

            Assert.AreEqual(@"Tiny Example Test {""Key1"":""A\n\rB"",""Key2"":""\u0000"",""Key3"":""\\""}", actual);
        }

        [TestMethod()]
        public void TinyPacket_Decode_02() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test {""Key1"":""A\n\rB"",""Key2"":""\u0000"",""Key3"":""\\""}"));
            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
            Assert.AreEqual("A\n\rB", actual["Key1"]);
            Assert.AreEqual("\0", actual["Key2"]);
            Assert.AreEqual("\\", actual["Key3"]);
        }


        [TestMethod()]
        public void TinyPacket_Encode_NullItem_01() {
            var target = new TinyPacket("Example", "Test");
            target["A"] = "1";
            target["B"] = null;
            target["C"] = "2";

            var actual = UTF8Encoding.UTF8.GetString(target.GetBytes(null, omitIdentifiers: true));

            Assert.AreEqual(@"Tiny Example Test {""A"":""1"",""B"":null,""C"":""2""}", actual);
        }

        [TestMethod()]
        public void TinyPacket_Decode_NullItem_01A() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test {""A"":""1"",""B"":null,""C"":""2""}"));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
            Assert.AreEqual("1", actual["A"]);
            Assert.AreEqual(null, actual["B"]);
            Assert.AreEqual("2", actual["C"]);
        }

        [TestMethod()]
        public void TinyPacket_Decode_NullItem_01B() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test { ""A"":""1"", ""B"" : null, ""C"":""2"" }"));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
            Assert.AreEqual("1", actual["A"]);
            Assert.AreEqual(null, actual["B"]);
            Assert.AreEqual("2", actual["C"]);
        }


        [TestMethod()]
        public void TinyPacket_Encode_Empty() {
            var target = new TinyPacket("Example", "Test");

            var actual = UTF8Encoding.UTF8.GetString(target.GetBytes(null, omitIdentifiers: true));

            Assert.AreEqual(@"Tiny Example Test {}", actual);
        }

        [TestMethod()]
        public void TinyPacket_Decode_Empty() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test  {} "));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
        }

        [TestMethod()]
        public void TinyPacket_Decode_Null() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test  null "));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
        }

        [TestMethod()]
        public void TinyPacket_Decode_MissingData_01() { //it is an error state, but we shall recognize it.
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test "));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
        }

        [TestMethod()]
        public void TinyPacket_Decode_MissingData_02() { //it is an error state, but we shall recognize it.
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test"));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
        }

        [TestMethod()]
        public void TinyPacket_Encode_Indexer() {
            var target = new TinyPacket("Example", "Test");
            target["A"] = "0";
            target["A"] = "1";

            var actual = UTF8Encoding.UTF8.GetString(target.GetBytes(null, omitIdentifiers: true));

            Assert.AreEqual(@"Tiny Example Test {""A"":""1""}", actual);
        }

        [TestMethod()]
        public void TinyPacket_Decode_Indexer() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example Test {""A"":""0"",""A"":""1"",""B"":""null"",""B"":null}"));

            Assert.AreEqual("Example", actual.Product);
            Assert.AreEqual("Test", actual.Operation);
            Assert.AreEqual("1", actual["A"]);
            Assert.AreEqual(null, actual["B"]);
            Assert.AreEqual(null, actual["C"]);
        }

        [TestMethod()]
        [ExpectedException(typeof(FormatException))]
        public void TinyPacket_Decode_Error_01() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny Example "));
        }

        [TestMethod()]
        [ExpectedException(typeof(FormatException))]
        public void TinyPacket_Decode_Error_02() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@"Tiny "));
        }

        [TestMethod()]
        [ExpectedException(typeof(FormatException))]
        public void TinyPacket_Decode_Error_03() {
            TinyPacket actual = TinyPacket.Parse(UTF8Encoding.UTF8.GetBytes(@""));
        }


        [TestMethod()]
        public void TinyPacket_NullItems() {
            var target = new TinyPacket("Example", "Test");
            Assert.AreEqual(null, target["Key1Text"]);
            Assert.IsNotNull(target.GetEnumerator());
        }

        [TestMethod()]
        public void TinyPacket_Encrypted() {
            var packet = new TinyPacket("Example", "Test");

            var key = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            var bytesE1 = packet.GetBytes(key);
            var bytesE2 = packet.GetBytes(key);

            Assert.AreEqual("Tiny128 ", Encoding.UTF8.GetString(bytesE1, 0, 8));
            Assert.AreEqual("Tiny128 ", Encoding.UTF8.GetString(bytesE2, 0, 8));
            Assert.AreNotEqual(BitConverter.ToString(bytesE1), BitConverter.ToString(bytesE2));

            var packet1a = TinyPacket.Parse(bytesE1, key);
            var packet2a = TinyPacket.Parse(bytesE2, key);
            Assert.IsNull(packet1a[".Id"]);
            Assert.IsNull(packet1a[".Host"]);
            Assert.IsNull(packet2a[".Id"]);
            Assert.IsNull(packet2a[".Host"]);

            var packet1b = packet1a.Clone();
            var packet2b = packet2a.Clone();
            Assert.IsNull(packet1b[".Id"]);
            Assert.IsNull(packet1b[".Host"]);
            Assert.IsNull(packet2b[".Id"]);
            Assert.IsNull(packet2b[".Host"]);

            var bytesP1 = packet1b.GetBytes(null, omitIdentifiers: true);
            var bytesP2 = packet2b.GetBytes(null, omitIdentifiers: true);
            Assert.AreEqual("Tiny ", Encoding.UTF8.GetString(bytesP1, 0, 5));
            Assert.AreEqual("Tiny ", Encoding.UTF8.GetString(bytesP2, 0, 5));
            Assert.AreEqual(Encoding.UTF8.GetString(bytesP1), Encoding.UTF8.GetString(bytesP2));
        }

        [TestMethod()]
        public void TinyPacket_EncryptedWithIdentifiers() {
            var packet = new TinyPacket("Example", "Test");

            var key = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            var bytesE1 = packet.GetBytes(key, omitIdentifiers: false);
            var bytesE2 = packet.GetBytes(key, omitIdentifiers: false);

            Assert.AreEqual("Tiny128 ", Encoding.UTF8.GetString(bytesE1, 0, 8));
            Assert.AreEqual("Tiny128 ", Encoding.UTF8.GetString(bytesE2, 0, 8));
            Assert.AreNotEqual(BitConverter.ToString(bytesE1), BitConverter.ToString(bytesE2));

            var packet1a = TinyPacket.Parse(bytesE1, key);
            var packet2a = TinyPacket.Parse(bytesE2, key);
            Assert.IsNull(packet1a[".Id"]);
            Assert.IsNotNull(packet1a[".Host"]);
            Assert.IsNull(packet2a[".Id"]);
            Assert.IsNotNull(packet2a[".Host"]);

            var packet1b = packet1a.Clone();
            var packet2b = packet2a.Clone();
            Assert.IsNull(packet1b[".Id"]);
            Assert.IsNull(packet1b[".Host"]);
            Assert.IsNull(packet2b[".Id"]);
            Assert.IsNull(packet2b[".Host"]);

            var bytesP1 = packet1b.GetBytes(null, omitIdentifiers: true);
            var bytesP2 = packet2b.GetBytes(null, omitIdentifiers: true);
            Assert.AreEqual("Tiny ", Encoding.UTF8.GetString(bytesP1, 0, 5));
            Assert.AreEqual("Tiny ", Encoding.UTF8.GetString(bytesP2, 0, 5));
            Assert.AreEqual(Encoding.UTF8.GetString(bytesP1), Encoding.UTF8.GetString(bytesP2));
        }

        [TestMethod()]
        public void TinyPacket_Cloning() {
            var packet = new TinyPacket("Example", "Test") { { "A", "1" }, { "B", "2" } };

            var key = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            var bytesE = packet.GetBytes(key, omitIdentifiers: false);

            var packetA = TinyPacket.Parse(bytesE, key);
            Assert.IsNull(packetA[".Id"]);
            Assert.IsNotNull(packetA[".Host"]);
            Assert.IsNotNull(packetA["A"]);
            Assert.IsNotNull(packetA["B"]);

            var packetB = packetA.Clone();
            Assert.IsNull(packetB[".Id"]);
            Assert.IsNull(packetB[".Host"]);
            Assert.IsNotNull(packetA["A"]);
            Assert.IsNotNull(packetA["B"]);

            var packetC = TinyPacket.Parse(packetB.GetBytes());
            Assert.IsNotNull(packetC[".Id"]);
            Assert.IsNotNull(packetC[".Host"]);
            Assert.IsNotNull(packetA["A"]);
            Assert.IsNotNull(packetA["B"]);

            var packetD = packetC.Clone();

            var bytesB = packetB.GetBytes(null, omitIdentifiers: true);
            var bytesD = packetD.GetBytes(null, omitIdentifiers: true);
            Assert.AreEqual(Encoding.UTF8.GetString(bytesB), Encoding.UTF8.GetString(bytesD));

        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TinyPacket_CannotAddSpecialProperties1() {
            var packet = new TinyPacket("Example", "Test") { { "A", "1" }, { "B", "2" } };
            packet.Add(".X", "X");
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TinyPacket_CannotAddSpecialProperties2() {
            var packet = new TinyPacket("Example", "Test") { { "A", "1" }, { "B", "2" } };
            packet[".X"] = "X";
        }


        [TestMethod()]
        public void TinyPacket_EncodeDecode_SpeedTest() {
            TinyPacket target = null;
            byte[] bytes = null;

            var nEncodePlain = 0;
            var swEncodePlain = Stopwatch.StartNew();
            while (swEncodePlain.ElapsedMilliseconds < 500) {
                target = new TinyPacket("Example", "Test"){
                    {"Key1Text", "Value1Text"},
                    {"Key2Text", "Value2Text"}
                };
                bytes = target.GetBytes();
                nEncodePlain += 1;
            }
            swEncodePlain.Stop();
            this.TestContext.WriteLine(string.Format("TinyPacket.Encode.Plain: {0:#,##0}K packets/second", (double)nEncodePlain / swEncodePlain.ElapsedMilliseconds));

            var nDecodePlain = 0;
            var swDecodePlain = Stopwatch.StartNew();
            while (swDecodePlain.ElapsedMilliseconds < 500) {
                var target2 = TinyPacket.Parse(bytes);
                nDecodePlain += 1;
            }
            swDecodePlain.Stop();
            this.TestContext.WriteLine(string.Format("TinyPacket.Decode.Plain: {0:#,##0}K packets/second", (double)nDecodePlain / swDecodePlain.ElapsedMilliseconds));

            var key = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

            var nEncodeAes = 0;
            var swEncodeAes = Stopwatch.StartNew();
            while (swEncodeAes.ElapsedMilliseconds < 500) {
                target = new TinyPacket("Example", "Test"){
                    {"Key1Text", "Value1Text"},
                    {"Key2Text", "Value2Text"}
                };
                bytes = target.GetBytes(key);
                nEncodeAes += 1;
            }
            swEncodeAes.Stop();
            this.TestContext.WriteLine(string.Format("TinyPacket.Encode.AES: {0:#,##0}K packets/second", (double)nEncodeAes / swEncodeAes.ElapsedMilliseconds));

            Assert.AreEqual("Tiny128 ", Encoding.UTF8.GetString(bytes, 0, 8));

            var nDecodeAes = 0;
            var swDecodeAes = Stopwatch.StartNew();
            while (swDecodeAes.ElapsedMilliseconds < 500) {
                var target2 = TinyPacket.Parse(bytes, key);
                nDecodeAes += 1;
            }
            swDecodeAes.Stop();
            this.TestContext.WriteLine(string.Format("TinyPacket.Decode.AES: {0:#,##0}K packets/second", (double)nDecodeAes / swDecodeAes.ElapsedMilliseconds));
        }

    }
}
