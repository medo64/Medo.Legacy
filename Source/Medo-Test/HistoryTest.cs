using Medo.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace Test {

    [TestClass()]
    public class HistoryTest {

        [TestMethod()]
        public void History_New() {
            var h = new History();
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(0, items.Length);
        }


        [TestMethod()]
        public void History_Clear() {
            var h = new History();
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);
            h.Clear();

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(0, items.Length);
        }


        [TestMethod()]
        public void History_Append() {
            var h = new History();
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);

            h.Append("A");
            h.Append("B");

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual("A", items[0]);
            Assert.AreEqual("B", items[1]);
        }

        [TestMethod()]
        public void History_AppendMore() {
            var h = new History(2);
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);

            h.Append("A");
            h.Append("B");
            h.Append("C");
            h.Append("D");

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual("C", items[0]);
            Assert.AreEqual("D", items[1]);
        }

        [TestMethod()]
        public void History_AppendLessDuplicates() {
            var h = new History(2);
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);

            h.Append("A");
            h.Append("A");
            h.Append("A");
            h.Append("A");

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("A", items[0]);
        }

        [TestMethod()]
        public void History_AppendDuplicates() {
            var h = new History(2);
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);

            h.Append("A");
            h.Append("B");
            h.Append("B");
            h.Append("A");

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual("B", items[0]);
            Assert.AreEqual("A", items[1]);
        }

        [TestMethod()]
        public void History_AppendCaseInsensitive() {
            var h = new History() { Comparer = StringComparer.OrdinalIgnoreCase };
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);

            h.Append("a");
            h.Append("b");
            h.Append("B");
            h.Append("A");

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual("B", items[0]);
            Assert.AreEqual("A", items[1]);
        }


        [TestMethod()]
        public void History_Prepend() {
            var h = new History();
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);

            h.Prepend("B");
            h.Prepend("A");

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual("A", items[0]);
            Assert.AreEqual("B", items[1]);
        }

        [TestMethod()]
        public void History_PrependMore() {
            var h = new History(2);
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);

            h.Prepend("D");
            h.Prepend("C");
            h.Prepend("B");
            h.Prepend("A");

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual("A", items[0]);
            Assert.AreEqual("B", items[1]);
        }

        [TestMethod()]
        public void History_PrependLessDuplicates() {
            var h = new History(2);
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);

            h.Prepend("A");
            h.Prepend("A");
            h.Prepend("A");
            h.Prepend("A");

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("A", items[0]);
        }

        [TestMethod()]
        public void History_PrependDuplicates() {
            var h = new History(2);
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);

            h.Prepend("A");
            h.Prepend("B");
            h.Prepend("B");
            h.Prepend("A");

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual("A", items[0]);
            Assert.AreEqual("B", items[1]);
        }

        [TestMethod()]
        public void History_PrependCaseInsensitive() {
            var h = new History() { Comparer = StringComparer.OrdinalIgnoreCase };
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);

            h.Prepend("b");
            h.Prepend("a");
            h.Prepend("B");
            h.Prepend("A");

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual("A", items[0]);
            Assert.AreEqual("B", items[1]);
        }


        [TestMethod()]
        public void History_Remove() {
            var h = new History();
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);

            h.Append("A");
            h.Append("B");
            h.Remove("A");

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("B", items[0]);
        }

        [TestMethod()]
        public void History_RemoveCaseInsensitive() {
            var h = new History() { Comparer = StringComparer.Ordinal };
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);

            h.Append("A");
            h.Append("B");

            h.Comparer = StringComparer.OrdinalIgnoreCase;
            h.Remove("a");

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("B", items[0]);
        }


        [TestMethod()]
        public void History_Enumerable() {
            var h = new History() { Comparer = StringComparer.OrdinalIgnoreCase };
            Registry.CurrentUser.DeleteSubKeyTree(h.Subkey, false);

            h.Prepend("b");
            h.Prepend("a");
            h.Prepend("B");
            h.Prepend("A");

            h.Comparer = StringComparer.OrdinalIgnoreCase;

            var items = GetArrayFromEnumerable(h.Items);
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual("A", items[0]);
            Assert.AreEqual("B", items[1]);
        }


        #region Helper

        private string[] GetArrayFromEnumerable(IEnumerable<string> items) {
            var list = new List<string>();
            foreach (var item in items) {
                list.Add(item);
            }
            return list.ToArray();
        }

        #endregion

    }
}
