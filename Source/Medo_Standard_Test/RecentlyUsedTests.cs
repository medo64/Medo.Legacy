using System.IO;
using Medo.Configuration;
using Xunit;

namespace Test {
    public class RecentlyUsedTests {

        [Fact(DisplayName = "RecentlyUsed: Basic")]
        public void Basic() {
            var recent = new RecentlyUsed();

            recent.Push(@"C:\test1.txt");
            recent.Push(@"C:\test2.txt");
            recent.Push(@"C:\test3.txt");
            recent.Push(@"C:\test4.txt");

            Assert.Equal(4, recent.Count);
            Assert.Equal(@"C:\test4.txt", recent[0].FileName);
            Assert.Equal(@"C:\test3.txt", recent[1].FileName);
            Assert.Equal(@"C:\test2.txt", recent[2].FileName);
            Assert.Equal(@"C:\test1.txt", recent[3].FileName);

            Assert.StartsWith(@"test4", recent[0].Title);
            Assert.StartsWith(@"test3", recent[1].Title);
            Assert.StartsWith(@"test2", recent[2].Title);
            Assert.StartsWith(@"test1", recent[3].Title);
        }

        [Fact(DisplayName = "RecentlyUsed: Limit count")]
        public void LimitCount() {
            var recent = new RecentlyUsed(null, 2);
            recent.Push(@"C:\test1.txt");
            recent.Push(@"C:\test2.txt");
            recent.Push(@"C:\test3.txt");
            recent.Push(@"C:\test4.txt");

            Assert.Equal(2, recent.Count);
            Assert.Equal(@"C:\test4.txt", recent[0].FileName);
            Assert.Equal(@"C:\test3.txt", recent[1].FileName);
        }

        [Fact(DisplayName = "RecentlyUsed: Duplicate entry")]
        public void DuplicateEntry() {
            var recent = new RecentlyUsed(null);
            recent.Push(@"C:\test1.txt");
            recent.Push(@"C:\test2.txt");
            recent.Push(@"C:\test1.txt");

            Assert.Equal(2, recent.Count);
            Assert.Equal(@"C:\test1.txt", recent[0].FileName);
            Assert.Equal(@"C:\test2.txt", recent[1].FileName);
        }

        [Fact(DisplayName = "RecentlyUsed: Invalid file name")]
        public void InvalidFileName() {
            var recent = new RecentlyUsed(null);
            recent.Push(@"\\\");

            Assert.Equal(0, recent.Count);
        }


        [Fact(DisplayName = "RecentlyUsed: File existance check")]
        public void FileExists() {
            var recent = new RecentlyUsed(null);

            var fileName = Path.GetTempFileName();
            try {
                recent.Push(fileName);
                Assert.True(recent[0].FileExists);
            } finally {
                File.Delete(fileName);
            }
            Assert.False(recent[0].FileExists);
        }

        [Fact(DisplayName = "RecentlyUsed: Changed")]
        public void ChangedEvent() {
            int changedCount = 0;

            var recent = new RecentlyUsed();
            recent.Changed += (o, i) => { changedCount++; };

            recent.Push(@"C:\test1.txt");
            Assert.Equal(1, changedCount);

            recent.Push(@"C:\test2.txt");
            Assert.Equal(2, changedCount);

            recent.Remove(@"C:\test2.txt");
            Assert.Equal(3, changedCount);

            recent.Remove(@"C:\test2.txt");
            Assert.Equal(3, changedCount);

            recent.Clear();
            Assert.Equal(4, changedCount);

            recent.Clear();
            Assert.Equal(4, changedCount);
        }

    }
}
