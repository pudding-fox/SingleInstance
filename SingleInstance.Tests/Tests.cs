using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace SingleInstance.Tests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public async Task Test001()
        {
            var id = Guid.NewGuid().ToString();
            using (var server = new Server(id))
            {
                var expected = "Hello World!";
                var actual = default(string);
                server.Message += (sender, e) => actual = e.Message;
                var client = new Client(id);
                await client.Send(expected).ConfigureAwait(false);
#if NET40
                await TaskEx.Delay(1000).ConfigureAwait(false);
#else
                await Task.Delay(1000).ConfigureAwait(false);
#endif
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
