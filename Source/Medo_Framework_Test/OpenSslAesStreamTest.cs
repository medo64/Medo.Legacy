using Medo.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Test {

    [TestClass()]
    public class OpenSslAesStreamTest {

        public TestContext TestContext { get; set; }


        private static readonly string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";


        [TestMethod()]
        public void OpenSslAesStream_Decrypt_Aes128Cbc() {
            using (var ms = new MemoryStream(Convert.FromBase64String("U2FsdGVkX1/qwxCoKB7RuqDjEjlYCKjHFsLcRImNOZr+AZ9xmWWJZVDj74J1c1qwVscV6VgIVEs7+q6ym+GJjBJBKnAytFZTpJdccE+e8jSSxgoquh/t0IssIkMIwi/VGmYcAARuAzYfM1VRlMVHH1bvsEt6G0u0F7Upcx/oxsoCWfHxPy0OtkiN7upOX/nU"))) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Read, 128, CipherMode.CBC)) {
                    var buffer = new byte[4096];
                    var len = x.Read(buffer, 0, buffer.Length);
                    var text = UTF8Encoding.UTF8.GetString(buffer, 0, len);
                    Assert.AreEqual(LoremIpsum, text);
                }
            }
        }

        [TestMethod()]
        public void OpenSslAesStream_Decrypt_Aes192Cbc() {
            using (var ms = new MemoryStream(Convert.FromBase64String("U2FsdGVkX1/HgSHxIdDzJZx80JG2wfCsP8xxO3p/G3f73TulBk+ClLEVBNQV2HYlx4eRwAmeZFm8euW1nXqg+qtIb/Tf2e+WphHz7DUq+t2MXLl8vmXb3f20SCiWlltFPjTVpEkbEa6x650/lepNDnzome4HMhv2QE5gnbyGZV6QZ3xPuMGzoQHhAXOFhDZf"))) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Read, 192, CipherMode.CBC)) {
                    var buffer = new byte[4096];
                    var len = x.Read(buffer, 0, buffer.Length);
                    var text = UTF8Encoding.UTF8.GetString(buffer, 0, len);
                    Assert.AreEqual(LoremIpsum, text);
                }
            }
        }

        [TestMethod()]
        public void OpenSslAesStream_Decrypt_Aes256Cbc() {
            using (var ms = new MemoryStream(Convert.FromBase64String("U2FsdGVkX1+KKMLOE6JigLPhJPK7QEUJwG+y84vwvu2qyWvOtHzwVLRIa875S2YY5Fn3TcgIiCKMEjVxe/HcLE5e0B7WgXTLwm/O6bzWQ/sc055TZpXB7kKyTjOxrojwkCdL9+RVQRBQ/wHgPDMppYuOaelectm5oveZtTzXuBm6a/mLQQLnKVSRAH59JSJv"))) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Read)) {
                    var buffer = new byte[4096];
                    var len = x.Read(buffer, 0, buffer.Length);
                    var text = UTF8Encoding.UTF8.GetString(buffer, 0, len);
                    Assert.AreEqual(LoremIpsum, text);
                }
            }
        }


        [TestMethod()]
        public void OpenSslAesStream_Decrypt_Aes128Ecb() {
            using (var ms = new MemoryStream(Convert.FromBase64String("U2FsdGVkX1+qIxrtnapUCyjLqttfuPLMxe11EQ3rR/oiESgQidByUFyejbZqMLHhwTfWnrm6KvCsctqhaH+WBSqDOgsn2ttxxVC2k+LRvYaadntDOYD+x/LzkhDaRDduXrI2vW6hloGCAs7lrrdjVGYTDyHicT1m6ByV3JaxiJzQqjsslvIzxWPtQwucjpP1"))) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Read, 128, CipherMode.ECB)) {
                    var buffer = new byte[4096];
                    var len = x.Read(buffer, 0, buffer.Length);
                    var text = UTF8Encoding.UTF8.GetString(buffer, 0, len);
                    Assert.AreEqual(LoremIpsum, text);
                }
            }
        }

        [TestMethod()]
        public void OpenSslAesStream_Decrypt_Aes192Ecb() {
            using (var ms = new MemoryStream(Convert.FromBase64String("U2FsdGVkX18K6feZiKN/83qpwUcJSF60qK9ZJw+rkNZ6EYNcAd1/Eyzfi4jmUUhwFlFkRsDNq7RfHYvHGWSWoKQ+Ky3L3ef6DQlhz4VuKSp0FiiuIrTfSL4EVdyvQUluunfI/l8XiZt33zN2XzVarUdOW58RJGmdavfvKuwuhEUjTMKGfusxdG3tjwy9thrz"))) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Read, 192, CipherMode.ECB)) {
                    var buffer = new byte[4096];
                    var len = x.Read(buffer, 0, buffer.Length);
                    var text = UTF8Encoding.UTF8.GetString(buffer, 0, len);
                    Assert.AreEqual(LoremIpsum, text);
                }
            }
        }

        [TestMethod()]
        public void OpenSslAesStream_Decrypt_Aes256Ecb() {
            using (var ms = new MemoryStream(Convert.FromBase64String("U2FsdGVkX19+hni7Ja6tY213SASOkIk/XW9LNeeAdQho5eS0HTvhakPAEJ0cIBSp4Fj+7A4l/a/LQ4eFuQyHs6cGdNJXeWZ94h+8hxlgSL3g5DoKIBwlxMM1i4AntuG9q03L+9+g4zg8Xhs9JCc50fapDvyqpauVTCI5ezJN+4+ypL9OQu71Qft4HMPnvEPk"))) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Read, 256, CipherMode.ECB)) {
                    var buffer = new byte[4096];
                    var len = x.Read(buffer, 0, buffer.Length);
                    var text = UTF8Encoding.UTF8.GetString(buffer, 0, len);
                    Assert.AreEqual(LoremIpsum, text);
                }
            }
        }

        [TestMethod()]
        public void OpenSslAesStream_Encrypt_Aes128Cbc() {
            byte[] bytes;
            using (var ms = new MemoryStream()) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Write, 128, CipherMode.CBC)) {
                    var buffer = UTF8Encoding.UTF8.GetBytes(LoremIpsum);
                    x.Write(buffer, 0, buffer.Length);
                }
                bytes = ms.ToArray();
            }
            using (var ms = new MemoryStream(bytes)) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Read, 128, CipherMode.CBC)) {
                    var buffer = new byte[4096];
                    var len = x.Read(buffer, 0, buffer.Length);
                    var text = UTF8Encoding.UTF8.GetString(buffer, 0, len);
                    Assert.AreEqual(LoremIpsum, text);
                }
            }
        }

        [TestMethod()]
        public void OpenSslAesStream_Encrypt_Aes192Cbc() {
            byte[] bytes;
            using (var ms = new MemoryStream()) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Write, 192, CipherMode.CBC)) {
                    var buffer = UTF8Encoding.UTF8.GetBytes(LoremIpsum);
                    x.Write(buffer, 0, buffer.Length);
                }
                bytes = ms.ToArray();
            }
            using (var ms = new MemoryStream(bytes)) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Read, 192, CipherMode.CBC)) {
                    var buffer = new byte[4096];
                    var len = x.Read(buffer, 0, buffer.Length);
                    var text = UTF8Encoding.UTF8.GetString(buffer, 0, len);
                    Assert.AreEqual(LoremIpsum, text);
                }
            }
        }

        [TestMethod()]
        public void OpenSslAesStream_Encrypt_Aes256Cbc() {
            byte[] bytes;
            using (var ms = new MemoryStream()) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Write, 256, CipherMode.CBC)) {
                    var buffer = UTF8Encoding.UTF8.GetBytes(LoremIpsum);
                    x.Write(buffer, 0, buffer.Length);
                }
                bytes = ms.ToArray();
            }
            using (var ms = new MemoryStream(bytes)) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Read, 256, CipherMode.CBC)) {
                    var buffer = new byte[4096];
                    var len = x.Read(buffer, 0, buffer.Length);
                    var text = UTF8Encoding.UTF8.GetString(buffer, 0, len);
                    Assert.AreEqual(LoremIpsum, text);
                }
            }
        }


        [TestMethod()]
        public void OpenSslAesStream_Encrypt_Aes128Ecb() {
            byte[] bytes;
            using (var ms = new MemoryStream()) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Write, 128, CipherMode.ECB)) {
                    var buffer = UTF8Encoding.UTF8.GetBytes(LoremIpsum);
                    x.Write(buffer, 0, buffer.Length);
                }
                bytes = ms.ToArray();
            }
            using (var ms = new MemoryStream(bytes)) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Read, 128, CipherMode.ECB)) {
                    var buffer = new byte[4096];
                    var len = x.Read(buffer, 0, buffer.Length);
                    var text = UTF8Encoding.UTF8.GetString(buffer, 0, len);
                    Assert.AreEqual(LoremIpsum, text);
                }
            }
        }

        [TestMethod()]
        public void OpenSslAesStream_Encrypt_Aes192Ecb() {
            byte[] bytes;
            using (var ms = new MemoryStream()) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Write, 192, CipherMode.ECB)) {
                    var buffer = UTF8Encoding.UTF8.GetBytes(LoremIpsum);
                    x.Write(buffer, 0, buffer.Length);
                }
                bytes = ms.ToArray();
            }
            using (var ms = new MemoryStream(bytes)) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Read, 192, CipherMode.ECB)) {
                    var buffer = new byte[4096];
                    var len = x.Read(buffer, 0, buffer.Length);
                    var text = UTF8Encoding.UTF8.GetString(buffer, 0, len);
                    Assert.AreEqual(LoremIpsum, text);
                }
            }
        }

        [TestMethod()]
        public void OpenSslAesStream_Encrypt_Aes256Ecb() {
            byte[] bytes;
            using (var ms = new MemoryStream()) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Write, 256, CipherMode.ECB)) {
                    var buffer = UTF8Encoding.UTF8.GetBytes(LoremIpsum);
                    x.Write(buffer, 0, buffer.Length);
                }
                bytes = ms.ToArray();
            }
            using (var ms = new MemoryStream(bytes)) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Read, 256, CipherMode.ECB)) {
                    var buffer = new byte[4096];
                    var len = x.Read(buffer, 0, buffer.Length);
                    var text = UTF8Encoding.UTF8.GetString(buffer, 0, len);
                    Assert.AreEqual(LoremIpsum, text);
                }
            }
        }


        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void OpenSslAesStream_InvalidCipherMode_01() {
            using (var ms = new MemoryStream()) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Write, 128, CipherMode.CFB)) {
                    var buffer = UTF8Encoding.UTF8.GetBytes(LoremIpsum);
                    x.Write(buffer, 0, buffer.Length);
                }
            }
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void OpenSslAesStream_InvalidCipherMode_02() {
            using (var ms = new MemoryStream()) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Write, 128, CipherMode.CTS)) {
                    var buffer = UTF8Encoding.UTF8.GetBytes(LoremIpsum);
                    x.Write(buffer, 0, buffer.Length);
                }
            }
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void OpenSslAesStream_InvalidCipherMode_03() {
            using (var ms = new MemoryStream()) {
                using (var x = new OpenSslAesStream(ms, "test", CryptoStreamMode.Write, 128, CipherMode.OFB)) {
                    var buffer = UTF8Encoding.UTF8.GetBytes(LoremIpsum);
                    x.Write(buffer, 0, buffer.Length);
                }
            }
        }

    }
}
