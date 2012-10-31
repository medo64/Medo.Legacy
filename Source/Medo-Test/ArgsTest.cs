using Medo.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class ArgsTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void Args_ContainsKey_1() {
            Args target = new Args("/a=X /b=1 /a=\"0 1\" file Name", false);
            bool result = target.ContainsKey("A");

            Assert.AreEqual(true, result);
        }

        [TestMethod()]
        public void Args_ContainsKey_2() {
            Args target = new Args("/a=X /b=1 /a=\"0 1\" file Name", false);
            bool result = target.ContainsKey("AB");

            Assert.AreEqual(false, result);
        }

        [TestMethod()]
        public void Args_GetKeys() {
            Args target = new Args("/a=X /b=1 /a=\"0 1\" file Name", false);
            string[] result = target.GetKeys();

            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("A", result[0]);
            Assert.AreEqual("B", result[1]);
            Assert.AreEqual("", result[2]);
        }

        [TestMethod()]
        public void Args_GetValue_String_1() {
            Args target = new Args("/a=X /b=1 /a=\"0 1\" file Name", false);
            string result = target.GetValue("A", "XX");

            Assert.AreEqual("0 1", result);
        }

        [TestMethod()]
        public void Args_GetValue_String_2() {
            Args target = new Args("/a=X /b=1 /a=\"0 1\" file Name", false);
            string result = target.GetValue("X", "XX");

            Assert.AreEqual("XX", result);
        }

        [TestMethod()]
        public void Args_GetValue_Bool_1() {
            Args target = new Args("/a=X /b=1 /a=\"0\" file Name", false);
            bool result = target.GetValue("A", true);

            Assert.AreEqual(false, result);
        }

        [TestMethod()]
        public void Args_GetValue_Bool_2() {
            Args target = new Args("/a=X /b=1 /a=\"faLse\" file Name", false);
            bool result = target.GetValue("A", true);

            Assert.AreEqual(false, result);
        }

        [TestMethod()]
        public void Args_GetValue_Bool_3() {
            Args target = new Args("/a=X /b=1 /a=\"0.123\" file Name", false);
            bool result = target.GetValue("X", true);

            Assert.AreEqual(true, result);
        }

        [TestMethod()]
        public void Args_GetValue_Bool_4() {
            Args target = new Args("/a=X /b=1 /a=\"0.123\" file Name", false);
            bool result = target.GetValue("A", true);

            Assert.AreEqual(true, result);
        }

        [TestMethod()]
        public void Args_GetValue_Int_1() {
            Args target = new Args("/a=X /b=1 /a=\"3\" file Name", false);
            int result = target.GetValue("A", 2);

            Assert.AreEqual(3, result);
        }

        [TestMethod()]
        public void Args_GetValue_Int_2() {
            Args target = new Args("/a=X /b=1 /a=\"3\" file Name", false);
            int result = target.GetValue("X", 2);

            Assert.AreEqual(2, result);
        }

        [TestMethod()]
        public void Args_GetValue_Int_3() {
            Args target = new Args("/a=X /b=1 /a=\"0.123\" file Name", false);
            double result = target.GetValue("A", 1);

            Assert.AreEqual(1, result);
        }

        [TestMethod()]
        public void Args_GetValue_Double_1() {
            Args target = new Args("/a=X /b=1 /a=\"3\" file Name", false);
            double result = target.GetValue("A", 1.00);

            Assert.AreEqual(3.00, result);
        }

        [TestMethod()]
        public void Args_GetValue_Double_2() {
            Args target = new Args("/a=X /b=1 /a=\"0.123\" file Name", false);
            double result = target.GetValue("X", 1);

            Assert.AreEqual(1, result);
        }

        [TestMethod()]
        public void Args_GetValue_1() {
            Args target = new Args("/a=0 /b=1 /a=\"x y\" fileName", false);
            string result = target.GetValue("A");

            Assert.AreEqual("x y", result);
        }

        [TestMethod()]
        public void Args_GetValue_2() {
            Args target = new Args("/a=0 /b=1 /a=\"x y\" fileName", false);
            string result = target.GetValue("Y");

            Assert.AreEqual(null, result);
        }

        [TestMethod()]
        public void Args_GetValues_First_1() {
            Args target = new Args("\"first one\" /a=0 /b=1 /a=\"x y\" \"File name\"", false);
            string[] results = target.GetValues(null);

            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("first one", results[0]);
            Assert.AreEqual("File name", results[1]);
        }

        [TestMethod()]
        public void Args_GetValues_First_2() {
            Args target = new Args("/a=0 /b=1 /a=\"x y\" \"File name\"", false);
            string[] results = target.GetValues(null);

            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("File name", results[0]);
        }

        [TestMethod()]
        public void Args_GetValues_Slash() {
            Args target = new Args("/a=0 /b=1 /a=\"x y\" fileName", false);
            string[] results = target.GetValues("A");

            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("0", results[0]);
            Assert.AreEqual("x y", results[1]);
        }

        [TestMethod()]
        public void Args_GetValues_Dash() {
            Args target = new Args("-a:0 -b:1 -a:\"x y\" fileName", false);
            string[] results = target.GetValues("A");

            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("0", results[0]);
            Assert.AreEqual("x y", results[1]);
        }

        [TestMethod()]
        public void Args_GetValues_DoubleDash() {
            Args target = new Args("--a=0 --b=1 --a:\"x y\" fileName", false);
            string[] results = target.GetValues("A");

            Assert.AreEqual(2, results.Length);
            Assert.AreEqual("0", results[0]);
            Assert.AreEqual("x y", results[1]);
        }

        [TestMethod()]
        public void Args_GetValue_ArrayInit_1() {
            Args target = new Args(new string[] { "Q:\\Projects\\Personal\\Home\\0.00\\Binaries\\HomeService.vshost.exe", "-servicedebug", "-x=file name" }, 1, 2);
            bool result1 = target.ContainsKey("ServiceDebug");
            string result2 = target.GetValue("x");
            int result3 = target.GetValue("g", 2);

            Assert.AreEqual(true, result1);
            Assert.AreEqual("file name", result2);
            Assert.AreEqual(2, result3);
        }

    }
}
