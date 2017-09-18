using System;
using Medo.Net;
using Xunit;

namespace Test {
    public class TrivialNtpClientTests {

        [Fact(DisplayName = "TrivialNtpClient: Basic")]
        void Basic() {
            var time = TrivialNtpClient.RetrieveTime("0.medo64.pool.ntp.org");
            var diff = DateTime.UtcNow - time;
            Assert.True(Math.Abs(diff.TotalSeconds) < 1);
        }

        [Fact(DisplayName = "TrivialNtpClient: Async")]
        async void Async() {
            var time = await TrivialNtpClient.RetrieveTimeAsync("0.medo64.pool.ntp.org");
            var diff = DateTime.UtcNow - time;
            Assert.True(Math.Abs(diff.TotalSeconds) < 1);
        }


        [Fact(DisplayName = "TrivialNtpClient: Timeout")]
        void Timeout() {
            using (var client = new TrivialNtpClient("0.medo64.pool.ntp.org") { Timeout = 1 }) {
                Assert.Throws<InvalidOperationException>(() => {
                    var time = client.RetrieveTime();
                });
            }
        }

        [Fact(DisplayName = "TrivialNtpClient: Timeout (async)")]
        async void TimeoutAsync() {
            using (var client = new TrivialNtpClient("0.medo64.pool.ntp.org") { Timeout = 1 }) {
                await Assert.ThrowsAsync<InvalidOperationException>(async () => {
                    var time = await client.RetrieveTimeAsync();
                });
            }
        }


        [Theory(DisplayName = "TrivialNtpClient: Invalid host")]
        [InlineData("")]
        [InlineData("  ")]
        void InvalidHostName(object data) {
            var hostName = data as string;
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                using (var client = new TrivialNtpClient(hostName)) { }
            });
        }

        [Theory(DisplayName = "TrivialNtpClient: Invalid port")]
        [InlineData(0)]
        [InlineData(65536)]
        void InvalidPort(object data) {
            int port = (int)data;
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                using (var client = new TrivialNtpClient("0.medo64.pool.ntp.org", port)) { }
            });
        }

        [Fact(DisplayName = "TrivialNtpClient: Non-existing host")]
        void NonExistingHost() {
            using (var client = new TrivialNtpClient("nonexisting.medo64.com")) {
                Assert.Throws<InvalidOperationException>(() => {
                    var time = client.RetrieveTime();
                });
            }
        }

        [Fact(DisplayName = "TrivialNtpClient: Non-existing host (async)")]
        async void NonExistingHostAsync() {
            using (var client = new TrivialNtpClient("nonexisting.medo64.com")) {
               await Assert.ThrowsAsync<InvalidOperationException>(async () => {
                    var time = await client.RetrieveTimeAsync();
                });
            }
        }

    }
}
