using Medo.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class Sha1Test {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void Sha1_ComputeHash() {
            byte[] result = Sha1.ComputeHash(new byte[] { 0, 1, 2, 3 }, 1, 2);
            string resultString = System.BitConverter.ToString(result);

            Assert.AreEqual("0C-A6-23-E2-85-5F-2C-75-C8-42-AD-30-2F-E8-20-E4-1B-4D-19-7D", resultString);
        }

    }
}
