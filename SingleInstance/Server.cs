﻿using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SingleInstance
{
    public class Server : IDisposable
    {
        public Server(string id)
        {
            var success = default(bool);
            this.Id  = id;
            this.Mutex = new Mutex(true, this.Id, out success);
            if (success)
            {
                Task.Factory.StartNew(() => this.OnStart());
            }
            else
            {
                this.Dispose();
            }
        }

        public string Id { get; private set; }  

        public Mutex Mutex { get; private set; }

        protected virtual async Task OnStart()
        {
            using (var server = new NamedPipeServerStream(Id))
            {
                using (var reader = new StreamReader(server))
                {
                    while (!this.IsDisposed)
                    {
#if NET40
                        server.WaitForConnection();
#else
                        await server.WaitForConnectionAsync().ConfigureAwait(false);
#endif
                        await this.OnConnection(server, reader).ConfigureAwait(false);
                    }
                }
            }
        }

        protected virtual async Task OnConnection(NamedPipeServerStream server, StreamReader reader)
        {
            var builder = new StringBuilder();
            while (server.IsConnected)
            {
                builder.Append(await reader.ReadToEndAsync().ConfigureAwait(false));
            }
            this.OnMessage(builder.ToString());
            server.Disconnect();
        }

        protected virtual void OnMessage(string message)
        {
            if (this.Message == null)
            {
                return;
            }
            this.Message(this, new ListenerEventArgs(message));
        }

        public event ListenerEventHandler Message;

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.IsDisposed || !disposing)
            {
                return;
            }
            this.OnDisposing();
            this.IsDisposed = true;
        }

        protected virtual void OnDisposing()
        {
            this.Mutex.Dispose();
        }

        ~Server()
        {
            try
            {
                this.Dispose(true);
            }
            catch
            {
                //Nothing can be done, never throw on GC thread.
            }
        }
    }

    public delegate void ListenerEventHandler(object sender, ListenerEventArgs e);

    public class ListenerEventArgs : EventArgs
    {
        public ListenerEventArgs(string message)
        {
            this.Message = message;
        }

        public string Message { get; private set; }
    }
}
