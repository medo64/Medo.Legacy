using Medo.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Test {

    [TestClass()]
    public class TwofishTest {

        [TestMethod()]
        public void Twofish_MonteCarlo_ECB() {
            var tests = GetTestBlocks(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.Twofish.ECB_TBL.TXT"));
            foreach (var test in tests) {
                using (var algorithm = new TwofishManaged() { KeySize = test.KeySize, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                    var ct = Encrypt(algorithm, test.Key, null, test.PlainText);
                    Assert.AreEqual(BitConverter.ToString(test.CipherText), BitConverter.ToString(ct), "Test Decrypt " + test.Index.ToString() + " (" + test.KeySize.ToString() + ")");

                    var pt = Decrypt(algorithm, test.Key, null, test.CipherText);
                    Assert.AreEqual(BitConverter.ToString(test.PlainText), BitConverter.ToString(pt), "Test Decrypt " + test.Index.ToString() + " (" + test.KeySize.ToString() + ")");
                }
            }
        }

        //[TestMethod()]
        public void Twofish_MonteCarlo_ECB_Encrypt() { //takes ages
            var tests = GetTestBlocks(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.Twofish.ECB_E_M.TXT"));
            var sw = Stopwatch.StartNew();
            foreach (var test in tests) {
                MonteCarlo_ECB_E(test);
            }
            sw.Stop();
            Assert.Inconclusive("Duration: " + sw.ElapsedMilliseconds.ToString() + " ms");
        }

        [TestMethod()]
        public void Twofish_MonteCarlo_ECB_Encrypt_One() {
            var tests = GetTestBlocks(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.Twofish.ECB_E_M.TXT"));
            var test = tests[Rnd.Next(tests.Count)];
            MonteCarlo_ECB_E(test);
        }


        //[TestMethod()]
        public void Twofish_MonteCarlo_ECB_Decrypt() { //takes ages
            var tests = GetTestBlocks(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.Twofish.ECB_D_M.TXT"));
            var sw = Stopwatch.StartNew();
            foreach (var test in tests) {
                MonteCarlo_ECB_D(test);
            }
            sw.Stop();
            Assert.Inconclusive("Duration: " + sw.ElapsedMilliseconds.ToString() + " ms");
        }

        [TestMethod()]
        public void Twofish_MonteCarlo_ECB_Decrypt_One() {
            var tests = GetTestBlocks(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.Twofish.ECB_D_M.TXT"));
            var test = tests[Rnd.Next(tests.Count)];
            MonteCarlo_ECB_D(test);
        }


        //[TestMethod()]
        public void Twofish_MonteCarlo_CBC_Encrypt() { //takes ages
            var tests = GetTestBlocks(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.Twofish.CBC_E_M.TXT"));
            var sw = Stopwatch.StartNew();
            foreach (var test in tests) {
                MonteCarlo_CBC_E(test);
            }
            sw.Stop();
            Assert.Inconclusive("Duration: " + sw.ElapsedMilliseconds.ToString() + " ms");
        }

        [TestMethod()]
        public void Twofish_MonteCarlo_CBC_Encrypt_One() {
            var tests = GetTestBlocks(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.Twofish.CBC_E_M.TXT"));
            var test = tests[Rnd.Next(tests.Count)];
            MonteCarlo_CBC_E(test);
        }


        //[TestMethod()]
        public void Twofish_MonteCarlo_CBC_Decrypt() { //takes ages
            var tests = GetTestBlocks(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.Twofish.CBC_D_M.TXT"));
            var sw = Stopwatch.StartNew();
            foreach (var test in tests) {
                MonteCarlo_CBC_D(test);
            }
            sw.Stop();
            Assert.Inconclusive("Duration: " + sw.ElapsedMilliseconds.ToString() + " ms");
        }

        [TestMethod()]
        public void Twofish_MonteCarlo_CBC_Decrypt_One() {
            var tests = GetTestBlocks(Assembly.GetExecutingAssembly().GetManifestResourceStream("Test.Twofish.CBC_D_M.TXT"));
            var test = tests[Rnd.Next(tests.Count)];
            MonteCarlo_CBC_D(test);
        }


        #region Private helper

        private static byte[] Encrypt(SymmetricAlgorithm algorithm, byte[] key, byte[] iv, byte[] pt) {
            using (var ms = new MemoryStream()) {
                using (var transform = algorithm.CreateEncryptor(key, iv)) {
                    using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Write)) {
                        cs.Write(pt, 0, pt.Length);
                    }
                }
                return ms.ToArray();
            }
        }

        private static byte[] Decrypt(SymmetricAlgorithm algorithm, byte[] key, byte[] iv, byte[] ct) {
            using (var ctStream = new MemoryStream(ct)) {
                using (var transform = algorithm.CreateDecryptor(key, iv)) {
                    using (var cs = new CryptoStream(ctStream, transform, CryptoStreamMode.Read)) {
                        using (var ms = new MemoryStream()) {
                            cs.CopyTo(ms);
                            return ms.ToArray();
                        }
                    }
                }
            }
        }

        #endregion

        #region Private: Monte carlo
        // http://www.ntua.gr/cryptix/old/cryptix/aes/docs/katmct.html

        private static void MonteCarlo_ECB_E(TestBlock test) {
            using (var algorithm = new TwofishManaged() { KeySize = test.KeySize, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                var key = test.Key;
                var pt = test.PlainText;
                byte[] ct = null;
                for (var j = 0; j < 10000; j++) {
                    ct = Encrypt(algorithm, key, null, pt);
                    pt = ct;
                }
                Assert.AreEqual(BitConverter.ToString(test.CipherText), BitConverter.ToString(ct), "Test " + test.Index.ToString() + " (" + test.KeySize.ToString() + ")");
            }
        }

        private static void MonteCarlo_ECB_D(TestBlock test) {
            using (var algorithm = new TwofishManaged() { KeySize = test.KeySize, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                var key = test.Key;
                var ct = test.CipherText;
                byte[] pt = null;
                for (var j = 0; j < 10000; j++) {
                    pt = Decrypt(algorithm, key, null, ct);
                    ct = pt;
                }
                Assert.AreEqual(BitConverter.ToString(test.PlainText), BitConverter.ToString(pt), "Test " + test.Index.ToString() + " (" + test.KeySize.ToString() + ")");
            }
        }


        private static void MonteCarlo_CBC_E(TestBlock test) {
            using (var algorithm = new TwofishManaged() { KeySize = test.KeySize, Mode = CipherMode.CBC, Padding = PaddingMode.None }) {
                var key = test.Key;
                var cv = test.IV;
                var pt = test.PlainText;
                byte[] ct = null;
                for (var j = 0; j < 10000; j++) {
                    var ob = Encrypt(algorithm, key, cv, pt);
                    pt = (j == 0) ? cv : ct;
                    ct = ob;
                    cv = ct;
                }
                Assert.AreEqual(BitConverter.ToString(test.CipherText), BitConverter.ToString(ct), "Test " + test.Index.ToString() + " (" + test.KeySize.ToString() + ")");
            }
        }

        private static void MonteCarlo_CBC_D(TestBlock test) {
            using (var algorithm = new TwofishManaged() { KeySize = test.KeySize, Mode = CipherMode.CBC, Padding = PaddingMode.None }) {
                var key = test.Key;
                var cv = test.IV;
                var ct = test.CipherText;
                byte[] pt = null;
                for (var j = 0; j < 10000; j++) {
                    pt = Decrypt(algorithm, key, cv, ct);
                    cv = ct;
                    ct = pt;
                }
                Assert.AreEqual(BitConverter.ToString(test.PlainText), BitConverter.ToString(pt), "Test " + test.Index.ToString() + " (" + test.KeySize.ToString() + ")");
            }
        }

        #endregion


        #region Multiblock

        [TestMethod()]
        public void Twofish_MultiBlock_ECB_128_Encrypt() {
            var key = ParseBytes("00000000000000000000000000000000");
            var pt = ParseBytes("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            using (var algorithm = new TwofishManaged() { KeySize = 128, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                var ct = Encrypt(algorithm, key, null, pt);
                Assert.AreEqual("9F589F5CF6122C32B6BFEC2F2AE8C35A9F589F5CF6122C32B6BFEC2F2AE8C35A9F589F5CF6122C32B6BFEC2F2AE8C35A", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_MultiBlock_ECB_128_Decrypt() {
            var key = ParseBytes("00000000000000000000000000000000");
            var ct = ParseBytes("9F589F5CF6122C32B6BFEC2F2AE8C35A9F589F5CF6122C32B6BFEC2F2AE8C35A9F589F5CF6122C32B6BFEC2F2AE8C35A");
            using (var algorithm = new TwofishManaged() { KeySize = 128, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                var pt = Decrypt(algorithm, key, null, ct);
                Assert.AreEqual("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", BitConverter.ToString(pt).Replace("-", ""));
            }
        }


        [TestMethod()]
        public void Twofish_MultiBlock_CBC_128_Encrypt() {
            var key = ParseBytes("00000000000000000000000000000000");
            var iv = ParseBytes("00000000000000000000000000000000");
            var pt = ParseBytes("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            using (var algorithm = new TwofishManaged() { KeySize = 128, Mode = CipherMode.CBC, Padding = PaddingMode.None }) {
                var ct = Encrypt(algorithm, key, iv, pt);
                Assert.AreEqual("9F589F5CF6122C32B6BFEC2F2AE8C35AD491DB16E7B1C39E86CB086B789F541905EF8C61A811582634BA5CB7106AA641", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_MultiBlock_CBC_128_Decrypt() {
            var key = ParseBytes("00000000000000000000000000000000");
            var iv = ParseBytes("00000000000000000000000000000000");
            var ct = ParseBytes("9F589F5CF6122C32B6BFEC2F2AE8C35AD491DB16E7B1C39E86CB086B789F541905EF8C61A811582634BA5CB7106AA641");
            using (var algorithm = new TwofishManaged() { KeySize = 128, Mode = CipherMode.CBC, Padding = PaddingMode.None }) {
                var pt = Decrypt(algorithm, key, iv, ct);
                Assert.AreEqual("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", BitConverter.ToString(pt).Replace("-", ""));
            }
        }


        [TestMethod()]
        public void Twofish_MultiBlock_ECB_192_Encrypt() {
            var key = ParseBytes("000000000000000000000000000000000000000000000000");
            var pt = ParseBytes("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            using (var algorithm = new TwofishManaged() { KeySize = 192, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                var ct = Encrypt(algorithm, key, null, pt);
                Assert.AreEqual("EFA71F788965BD4453F860178FC19101EFA71F788965BD4453F860178FC19101EFA71F788965BD4453F860178FC19101", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_MultiBlock_ECB_192_Decrypt() {
            var key = ParseBytes("000000000000000000000000000000000000000000000000");
            var ct = ParseBytes("EFA71F788965BD4453F860178FC19101EFA71F788965BD4453F860178FC19101EFA71F788965BD4453F860178FC19101");
            using (var algorithm = new TwofishManaged() { KeySize = 192, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                var pt = Decrypt(algorithm, key, null, ct);
                Assert.AreEqual("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", BitConverter.ToString(pt).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_MultiBlock_CBC_192_Encrypt() {
            var key = ParseBytes("000000000000000000000000000000000000000000000000");
            var iv = ParseBytes("00000000000000000000000000000000");
            var pt = ParseBytes("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            using (var algorithm = new TwofishManaged() { KeySize = 192, Mode = CipherMode.CBC, Padding = PaddingMode.None }) {
                var ct = Encrypt(algorithm, key, iv, pt);
                Assert.AreEqual("EFA71F788965BD4453F860178FC1910188B2B2706B105E36B446BB6D731A1E88F2DD994D2C4E64517CC9DB9AED2D5909", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_MultiBlock_CBC_192_Decrypt() {
            var key = ParseBytes("000000000000000000000000000000000000000000000000");
            var iv = ParseBytes("00000000000000000000000000000000");
            var ct = ParseBytes("EFA71F788965BD4453F860178FC1910188B2B2706B105E36B446BB6D731A1E88F2DD994D2C4E64517CC9DB9AED2D5909");
            using (var algorithm = new TwofishManaged() { KeySize = 192, Mode = CipherMode.CBC, Padding = PaddingMode.None }) {
                var pt = Decrypt(algorithm, key, iv, ct);
                Assert.AreEqual("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", BitConverter.ToString(pt).Replace("-", ""));
            }
        }


        [TestMethod()]
        public void Twofish_MultiBlock_ECB_256_Encrypt() {
            var key = ParseBytes("0000000000000000000000000000000000000000000000000000000000000000");
            var pt = ParseBytes("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            using (var algorithm = new TwofishManaged() { KeySize = 256, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                var ct = Encrypt(algorithm, key, null, pt);
                Assert.AreEqual("57FF739D4DC92C1BD7FC01700CC8216F57FF739D4DC92C1BD7FC01700CC8216F57FF739D4DC92C1BD7FC01700CC8216F", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_MultiBlock_ECB_256_Decrypt() {
            var key = ParseBytes("0000000000000000000000000000000000000000000000000000000000000000");
            var ct = ParseBytes("57FF739D4DC92C1BD7FC01700CC8216F57FF739D4DC92C1BD7FC01700CC8216F57FF739D4DC92C1BD7FC01700CC8216F");
            using (var algorithm = new TwofishManaged() { KeySize = 256, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                var pt = Decrypt(algorithm, key, null, ct);
                Assert.AreEqual("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", BitConverter.ToString(pt).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_MultiBlock_CBC_256_Encrypt() {
            var key = ParseBytes("0000000000000000000000000000000000000000000000000000000000000000");
            var iv = ParseBytes("00000000000000000000000000000000");
            var pt = ParseBytes("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            using (var algorithm = new TwofishManaged() { KeySize = 256, Mode = CipherMode.CBC, Padding = PaddingMode.None }) {
                var ct = Encrypt(algorithm, key, iv, pt);
                Assert.AreEqual("57FF739D4DC92C1BD7FC01700CC8216FD43BB7556EA32E46F2A282B7D45B4E0D2804E32925D62BAE74487A06B3CD2D46", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_MultiBlock_CBC_256_Decrypt() {
            var key = ParseBytes("0000000000000000000000000000000000000000000000000000000000000000");
            var iv = ParseBytes("00000000000000000000000000000000");
            var ct = ParseBytes("57FF739D4DC92C1BD7FC01700CC8216FD43BB7556EA32E46F2A282B7D45B4E0D2804E32925D62BAE74487A06B3CD2D46");
            using (var algorithm = new TwofishManaged() { KeySize = 256, Mode = CipherMode.CBC, Padding = PaddingMode.None }) {
                var pt = Decrypt(algorithm, key, iv, ct);
                Assert.AreEqual("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", BitConverter.ToString(pt).Replace("-", ""));
            }
        }


        [TestMethod()]
        public void Twofish_MultiBlockNonFinal_ECB_256_Encrypt() {
            var key = ParseBytes("0000000000000000000000000000000000000000000000000000000000000000");
            var pt = ParseBytes("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            using (var algorithm = new TwofishManaged() { KeySize = 256, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                algorithm.Key = key;
                var ct = new byte[pt.Length];
                using (var transform = algorithm.CreateEncryptor()) {
                    transform.TransformBlock(pt, 0, pt.Length, ct, 0);
                    transform.TransformFinalBlock(new byte[] { }, 0, 0);
                }
                Assert.AreEqual("57FF739D4DC92C1BD7FC01700CC8216F57FF739D4DC92C1BD7FC01700CC8216F57FF739D4DC92C1BD7FC01700CC8216F", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_MultiBlockNotFinal_ECB_256_Decrypt() {
            var key = ParseBytes("0000000000000000000000000000000000000000000000000000000000000000");
            var ct = ParseBytes("57FF739D4DC92C1BD7FC01700CC8216F57FF739D4DC92C1BD7FC01700CC8216F57FF739D4DC92C1BD7FC01700CC8216F");
            using (var algorithm = new TwofishManaged() { KeySize = 256, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                algorithm.Key = key;
                var pt = new byte[ct.Length];
                using (var transform = algorithm.CreateDecryptor()) {
                    transform.TransformBlock(ct, 0, ct.Length, pt, 0);
                    transform.TransformFinalBlock(new byte[] { }, 0, 0);
                }
                Assert.AreEqual("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", BitConverter.ToString(pt).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_MultiBlockFinal_ECB_256_Encrypt() {
            var key = ParseBytes("0000000000000000000000000000000000000000000000000000000000000000");
            var pt = ParseBytes("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000");
            using (var algorithm = new TwofishManaged() { KeySize = 256, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                algorithm.Key = key;
                var ct = algorithm.CreateEncryptor().TransformFinalBlock(pt, 0, pt.Length);
                Assert.AreEqual("57FF739D4DC92C1BD7FC01700CC8216F57FF739D4DC92C1BD7FC01700CC8216F57FF739D4DC92C1BD7FC01700CC8216F", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_MultiBlockFinal_ECB_256_Decrypt() {
            var key = ParseBytes("0000000000000000000000000000000000000000000000000000000000000000");
            var ct = ParseBytes("57FF739D4DC92C1BD7FC01700CC8216F57FF739D4DC92C1BD7FC01700CC8216F57FF739D4DC92C1BD7FC01700CC8216F");
            using (var algorithm = new TwofishManaged() { KeySize = 256, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                algorithm.Key = key;
                var pt = algorithm.CreateDecryptor().TransformFinalBlock(ct, 0, ct.Length);
                Assert.AreEqual("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", BitConverter.ToString(pt).Replace("-", ""));
            }
        }


        [TestMethod()]
        public void Twofish_MultiBlockNonFinal_CBC_256_Encrypt() {
            var key = ParseBytes("0000000000000000000000000000000000000000000000000000000000000000");
            var iv = ParseBytes("00000000000000000000000000000000");
            var pt = ParseBytes("9F589F5CF6122C32B6BFEC2F2AE8C35A9F589F5CF6122C32B6BFEC2F2AE8C35A9F589F5CF6122C32B6BFEC2F2AE8C35A");
            using (var algorithm = new TwofishManaged() { KeySize = 256, Mode = CipherMode.CBC, Padding = PaddingMode.None }) {
                algorithm.Key = key;
                algorithm.IV = iv;
                var ct = new byte[pt.Length];
                using (var transform = algorithm.CreateEncryptor()) {
                    transform.TransformBlock(pt, 0, pt.Length, ct, 0);
                    transform.TransformFinalBlock(new byte[] { }, 0, 0);
                }
                Assert.AreEqual("61B5BC459C4E9491DD9E6ACB7478813047BE7250D34F792C17F0C23583C0B040B95C9FAE11107EE9BAC3D79BBFE019EE", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_MultiBlockNonFinal_CBC_256_Decrypt() {
            var key = ParseBytes("0000000000000000000000000000000000000000000000000000000000000000");
            var iv = ParseBytes("00000000000000000000000000000000");
            var ct = ParseBytes("61B5BC459C4E9491DD9E6ACB7478813047BE7250D34F792C17F0C23583C0B040B95C9FAE11107EE9BAC3D79BBFE019EE");
            using (var algorithm = new TwofishManaged() { KeySize = 256, Mode = CipherMode.CBC, Padding = PaddingMode.None }) {
                algorithm.Key = key;
                algorithm.IV = iv;
                var pt = new byte[ct.Length]; pt[ct.Length - 1] = 0xFF;
                using (var transform = algorithm.CreateDecryptor()) {
                    transform.TransformBlock(ct, 0, ct.Length, pt, 0);
                    transform.TransformFinalBlock(new byte[] { }, 0, 0);
                }
                Assert.AreEqual("9F589F5CF6122C32B6BFEC2F2AE8C35A9F589F5CF6122C32B6BFEC2F2AE8C35A9F589F5CF6122C32B6BFEC2F2AE8C35A", BitConverter.ToString(pt).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_MultiBlockFinal_CBC_256_Encrypt() {
            var key = ParseBytes("0000000000000000000000000000000000000000000000000000000000000000");
            var iv = ParseBytes("00000000000000000000000000000000");
            var pt = ParseBytes("9F589F5CF6122C32B6BFEC2F2AE8C35A9F589F5CF6122C32B6BFEC2F2AE8C35A9F589F5CF6122C32B6BFEC2F2AE8C35A");
            using (var algorithm = new TwofishManaged() { KeySize = 256, Mode = CipherMode.CBC, Padding = PaddingMode.None }) {
                algorithm.Key = key;
                algorithm.IV = iv;
                var ct = algorithm.CreateEncryptor().TransformFinalBlock(pt, 0, pt.Length);
                Assert.AreEqual("61B5BC459C4E9491DD9E6ACB7478813047BE7250D34F792C17F0C23583C0B040B95C9FAE11107EE9BAC3D79BBFE019EE", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_MultiBlockFinal_CBC_256_Decrypt() {
            var key = ParseBytes("0000000000000000000000000000000000000000000000000000000000000000");
            var iv = ParseBytes("00000000000000000000000000000000");
            var ct = ParseBytes("61B5BC459C4E9491DD9E6ACB7478813047BE7250D34F792C17F0C23583C0B040B95C9FAE11107EE9BAC3D79BBFE019EE");
            using (var algorithm = new TwofishManaged() { KeySize = 256, Mode = CipherMode.CBC, Padding = PaddingMode.None }) {
                algorithm.Key = key;
                algorithm.IV = iv;
                var pt = algorithm.CreateDecryptor().TransformFinalBlock(ct, 0, ct.Length);
                Assert.AreEqual("9F589F5CF6122C32B6BFEC2F2AE8C35A9F589F5CF6122C32B6BFEC2F2AE8C35A9F589F5CF6122C32B6BFEC2F2AE8C35A", BitConverter.ToString(pt).Replace("-", ""));
            }
        }

        #endregion


        #region Padding

        [TestMethod()]
        public void Twofish_Padding_Zeros_ECB_128_Encrypt() {
            var key = ParseBytes("00000000000000000000000000000000");
            var pt = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");
            using (var algorithm = new TwofishManaged() { KeySize = 128, Mode = CipherMode.ECB, Padding = PaddingMode.Zeros }) {
                var ct = Encrypt(algorithm, key, null, pt);
                Assert.AreEqual("B0DD30E9AB1F1329C1BEE154DDBE88AF1194B36D8E0BDD5AC10842B549230BB33C25C273BF09B94A31DE3C27C28DFB5C", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_Padding_Zeros_ECB_128_Decrypt() {
            var key = ParseBytes("00000000000000000000000000000000");
            var ct = ParseBytes("B0DD30E9AB1F1329C1BEE154DDBE88AF1194B36D8E0BDD5AC10842B549230BB33C25C273BF09B94A31DE3C27C28DFB5C");
            using (var algorithm = new TwofishManaged() { KeySize = 128, Mode = CipherMode.ECB, Padding = PaddingMode.Zeros }) {
                var pt = Decrypt(algorithm, key, null, ct);
                Assert.AreEqual("The quick brown fox jumps over the lazy dog", Encoding.UTF8.GetString(pt));
            }
        }

        [TestMethod()]
        public void Twofish_Padding_None_ECB_128_Encrypt_16() {
            var key = ParseBytes("00000000000000000000000000000000");
            var pt = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog once");
            using (var algorithm = new TwofishManaged() { KeySize = 128, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                var ct = Encrypt(algorithm, key, null, pt);
                Assert.AreEqual("B0DD30E9AB1F1329C1BEE154DDBE88AF1194B36D8E0BDD5AC10842B549230BB36D66FC3AFE1F40216590079AF862AB59", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_Padding_None_ECB_128_Decrypt_16() {
            var key = ParseBytes("00000000000000000000000000000000");
            var ct = ParseBytes("B0DD30E9AB1F1329C1BEE154DDBE88AF1194B36D8E0BDD5AC10842B549230BB36D66FC3AFE1F40216590079AF862AB59");
            using (var algorithm = new TwofishManaged() { KeySize = 128, Mode = CipherMode.ECB, Padding = PaddingMode.None }) {
                var pt = Decrypt(algorithm, key, null, ct);
                Assert.AreEqual("The quick brown fox jumps over the lazy dog once", Encoding.UTF8.GetString(pt));
            }
        }


        [TestMethod()]
        public void Twofish_Padding_Zeros_ECB_128_Encrypt_16() {
            var key = ParseBytes("00000000000000000000000000000000");
            var pt = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog once");
            using (var algorithm = new TwofishManaged() { KeySize = 128, Mode = CipherMode.ECB, Padding = PaddingMode.Zeros }) {
                var ct = Encrypt(algorithm, key, null, pt);
                Assert.AreEqual("B0DD30E9AB1F1329C1BEE154DDBE88AF1194B36D8E0BDD5AC10842B549230BB36D66FC3AFE1F40216590079AF862AB59", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_Padding_Zeros_ECB_128_Decrypt_16() {
            var key = ParseBytes("00000000000000000000000000000000");
            var ct = ParseBytes("B0DD30E9AB1F1329C1BEE154DDBE88AF1194B36D8E0BDD5AC10842B549230BB36D66FC3AFE1F40216590079AF862AB59");
            using (var algorithm = new TwofishManaged() { KeySize = 128, Mode = CipherMode.ECB, Padding = PaddingMode.Zeros }) {
                var pt = Decrypt(algorithm, key, null, ct);
                Assert.AreEqual("The quick brown fox jumps over the lazy dog once", Encoding.UTF8.GetString(pt));
            }
        }


        [TestMethod()]
        public void Twofish_Padding_Pkcs7_ECB_128_Encrypt() {
            var key = ParseBytes("00000000000000000000000000000000");
            var pt = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog");
            using (var algorithm = new TwofishManaged() { KeySize = 128, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 }) {
                var ct = Encrypt(algorithm, key, null, pt);
                Assert.AreEqual("B0DD30E9AB1F1329C1BEE154DDBE88AF1194B36D8E0BDD5AC10842B549230BB3235D2E6063F32DE35B8A62A384FC587E", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_Padding_Pkcs7_ECB_128_Decrypt() {
            var key = ParseBytes("00000000000000000000000000000000");
            var ct = ParseBytes("B0DD30E9AB1F1329C1BEE154DDBE88AF1194B36D8E0BDD5AC10842B549230BB3235D2E6063F32DE35B8A62A384FC587E");
            using (var algorithm = new TwofishManaged() { KeySize = 128, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 }) {
                var pt = Decrypt(algorithm, key, null, ct);
                Assert.AreEqual("The quick brown fox jumps over the lazy dog", Encoding.UTF8.GetString(pt));
            }
        }

        [TestMethod()]
        public void Twofish_Padding_Pkcs7_ECB_128_Encrypt_16() {
            var key = ParseBytes("00000000000000000000000000000000");
            var pt = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog once");
            using (var algorithm = new TwofishManaged() { KeySize = 128, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 }) {
                var ct = Encrypt(algorithm, key, null, pt);
                Assert.AreEqual("B0DD30E9AB1F1329C1BEE154DDBE88AF1194B36D8E0BDD5AC10842B549230BB36D66FC3AFE1F40216590079AF862AB59771D591428AF301D69FA1E227D083527", BitConverter.ToString(ct).Replace("-", ""));
            }
        }

        [TestMethod()]
        public void Twofish_Padding_Pkcs7_ECB_128_Decrypt_16() {
            var key = ParseBytes("00000000000000000000000000000000");
            var ct = ParseBytes("B0DD30E9AB1F1329C1BEE154DDBE88AF1194B36D8E0BDD5AC10842B549230BB36D66FC3AFE1F40216590079AF862AB59771D591428AF301D69FA1E227D083527");
            using (var algorithm = new TwofishManaged() { KeySize = 128, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 }) {
                var pt = Decrypt(algorithm, key, null, ct);
                Assert.AreEqual("The quick brown fox jumps over the lazy dog once", Encoding.UTF8.GetString(pt));
            }
        }

        #endregion


        #region Other

        [TestMethod()]
        public void Twofish_TransformBlock_Encrypt_CorrectWrittenBytes() {
            var key = ParseBytes("00000000000000000000000000000000");
            var pt = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog once");
            var ct = new byte[48];
            using (var twofish = new TwofishManaged() { Mode = CipherMode.ECB, Padding = PaddingMode.None, KeySize = 128, Key = key }) {
                using (var transform = twofish.CreateEncryptor()) {
                    Assert.AreEqual(16, transform.TransformBlock(pt, 0, 16, ct, 0));
                }
            }
            using (var twofish = new TwofishManaged() { Mode = CipherMode.ECB, Padding = PaddingMode.None, KeySize = 128, Key = key }) {
                using (var transform = twofish.CreateEncryptor()) {
                    Assert.AreEqual(16, transform.TransformBlock(pt, 16, 16, ct, 16));
                }
            }
            using (var twofish = new TwofishManaged() { Mode = CipherMode.ECB, Padding = PaddingMode.None, KeySize = 128, Key = key }) {
                using (var transform = twofish.CreateEncryptor()) {
                    Assert.AreEqual(16, transform.TransformBlock(pt, 32, 16, ct, 32));
                }
            }
            Assert.AreEqual("B0DD30E9AB1F1329C1BEE154DDBE88AF1194B36D8E0BDD5AC10842B549230BB36D66FC3AFE1F40216590079AF862AB59", BitConverter.ToString(ct).Replace("-", ""));
        }

        [TestMethod()]
        public void Twofish_TransformBlock_Decrypt_CorrectWrittenBytes() {
            var key = ParseBytes("00000000000000000000000000000000");
            var ct = ParseBytes("B0DD30E9AB1F1329C1BEE154DDBE88AF1194B36D8E0BDD5AC10842B549230BB36D66FC3AFE1F40216590079AF862AB59");
            var pt = new byte[48];
            using (var twofish = new TwofishManaged() { Mode = CipherMode.ECB, Padding = PaddingMode.None, KeySize = 128, Key = key }) {
                using (var transform = twofish.CreateDecryptor()) {
                    Assert.AreEqual(16, transform.TransformBlock(ct, 0, 16, pt, 0)); //no caching last block if Padding is none
                }
            }
            using (var twofish = new TwofishManaged() { Mode = CipherMode.ECB, Padding = PaddingMode.None, KeySize = 128, Key = key }) {
                using (var transform = twofish.CreateDecryptor()) {
                    Assert.AreEqual(16, transform.TransformBlock(ct, 16, 16, pt, 16));
                }
            }
            using (var twofish = new TwofishManaged() { Mode = CipherMode.ECB, Padding = PaddingMode.None, KeySize = 128, Key = key }) {
                using (var transform = twofish.CreateDecryptor()) {
                    Assert.AreEqual(16, transform.TransformBlock(ct, 32, 16, pt, 32));
                }
            }
            Assert.AreEqual("The quick brown fox jumps over the lazy dog once", Encoding.UTF8.GetString(pt));
        }

        #endregion


        #region Private setup

        private static Random Rnd = new Random();

        private static List<TestBlock> GetTestBlocks(Stream fileStream) {
            var result = new List<TestBlock>();

            using (var s = new StreamReader(fileStream)) {
                int? keySize = null, i = null;
                byte[] key = null, iv = null, ct = null, pt = null;

                while (!s.EndOfStream) {
                    var line = s.ReadLine();
                    if (line.StartsWith("KEYSIZE=", StringComparison.Ordinal)) {
                        keySize = int.Parse(line.Substring(8), CultureInfo.InvariantCulture);
                        i = null;
                    } else if (line.StartsWith("I=", StringComparison.Ordinal)) {
                        if (keySize == null) { continue; }
                        i = int.Parse(line.Substring(2), CultureInfo.InvariantCulture);
                    } else if (line.StartsWith("KEY=", StringComparison.Ordinal)) {
                        key = ParseBytes(line.Substring(4));
                    } else if (line.StartsWith("IV=", StringComparison.Ordinal)) {
                        iv = ParseBytes(line.Substring(3));
                    } else if (line.StartsWith("PT=", StringComparison.Ordinal)) {
                        pt = ParseBytes(line.Substring(3));
                    } else if (line.StartsWith("CT=", StringComparison.Ordinal)) {
                        ct = ParseBytes(line.Substring(3));
                    } else if (line.Equals("", StringComparison.Ordinal)) {
                        if (i == null) { continue; }
                        result.Add(new TestBlock(keySize.Value, i.Value, key, iv, pt, ct));
                        i = null; key = null; iv = null; ct = null; pt = null;
                    }
                }
            }

            return result;
        }

        private static byte[] ParseBytes(string hex) {
            Trace.Assert((hex.Length % 2) == 0);
            byte[] result = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2) {
                result[i / 2] = byte.Parse(hex.Substring(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return result;
        }

        [DebuggerDisplay("{KeySize}:{Index}")]
        private struct TestBlock {
            internal TestBlock(int keySize, int index, byte[] key, byte[] iv, byte[] plainText, byte[] cipherText) {
                this.KeySize = keySize;
                this.Index = index;
                this.Key = key;
                this.IV = iv;
                this.PlainText = plainText;
                this.CipherText = cipherText;
            }
            internal int KeySize { get; }
            internal int Index { get; }
            internal byte[] Key { get; }
            internal byte[] IV { get; }
            internal byte[] PlainText { get; }
            internal byte[] CipherText { get; }
        }

        #endregion

    }
}
