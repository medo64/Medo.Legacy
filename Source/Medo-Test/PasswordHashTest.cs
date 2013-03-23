using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Medo.Security.Cryptography;

namespace Test {


    [TestClass()]
    public class PasswordHashTest {

        public TestContext TestContext { get; set; }

        [TestMethod()]
        public void PasswordHash_CreateAndVerify() {
            var x = PasswordHash.EncodePassword("Pass", "User");
            Assert.AreEqual(true, PasswordHash.CheckPassword(x, "Pass", "User"));
        }

        [TestMethod()]
        public void PasswordHash_Verify() {
            var x = PasswordHash.EncodePassword("Pass", "User");
            Assert.AreEqual(true, PasswordHash.CheckPassword("DRZCuiIvYaR18yS3HdgclFD+Na5ZrTtCR/yW87E6", "Pass", "User"));
        }

    }
}
