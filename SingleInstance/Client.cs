using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace SingleInstance
{
    public class Client
    {
        public Client(string id)
        {
            this.Id = id;
        }

        public string Id { get; private set; }

        public async Task Send(string message)
        {
            using (var client = new NamedPipeClientStream(this.Id))
            {
                using (var writer = new StreamWriter(client))
                {
#if NET40
                    client.Connect();
#else
                    await client.ConnectAsync().ConfigureAwait(false);
#endif
                    await writer.WriteAsync(message).ConfigureAwait(false);
                }
            }
        }
    }
}
