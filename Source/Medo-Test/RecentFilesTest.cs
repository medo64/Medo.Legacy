using Medo.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace Test {

    [TestClass()]
    public class RecentFilesTest {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void RecentFiles_01() {
            var x = new RecentFiles();
            x.Clear();
            x.Push(@"C:\test.txt");
            x.Push(@"C:\test.txt");
            Assert.AreEqual(1, x.Count);
            Assert.AreEqual(@"C:\test.txt", x[0].FileName);
        }

        [TestMethod()]
        public void RecentFiles_02() {
            var x = new RecentFiles();
            x.Clear();
            x.Push(@"C:\testA.txt");
            x.Push(@"C:\testB.txt");
            x.Push(@"C:\testA.txt");
            Assert.AreEqual(2, x.Count);
            Assert.AreEqual(@"C:\testA.txt", x[0].FileName);
            Assert.AreEqual(@"C:\testB.txt", x[1].FileName);
        }

        [TestMethod()]
        public void RecentFiles_03() {
            var x = new RecentFiles(1);
            x.Clear();
            x.Push(@"C:\testA.txt");
            x.Push(@"C:\testB.txt");
            x.Push(@"C:\testA.txt");
            Assert.AreEqual(1, x.Count);
            Assert.AreEqual(@"C:\testA.txt", x[0].FileName);
        }

        [TestMethod()]
        public void RecentFiles_04() {
            var x = new RecentFiles();
            x.Clear();
            Medo.Configuration.RecentFiles.NoRegistryWrites = true;
            x.Push(@"C:\testA.txt");
            x.Push(@"C:\testB.txt");
            x.Push(@"C:\testA.txt");
            Assert.AreEqual(2, x.Count);
            Assert.AreEqual(@"C:\testA.txt", x[0].FileName);
            Assert.AreEqual(@"C:\testB.txt", x[1].FileName);
            x.Load();
            Assert.AreEqual(0, x.Count);
        }


        [TestMethod()]
        public void RecentFiles_05() {
            var recentFiles = new RecentFiles();
            recentFiles.Clear();
            RecentFiles.NoRegistryWrites = true;
            recentFiles.Push(@"3");
            recentFiles.Push(@"2");
            recentFiles.Push(@"1");
            Assert.AreEqual(3, recentFiles.Count);

            var i = 0;
            foreach (var file in recentFiles) {
                i++;
                Assert.AreEqual(i.ToString(CultureInfo.InvariantCulture), file.FileName);
            }
        }

    }
}
