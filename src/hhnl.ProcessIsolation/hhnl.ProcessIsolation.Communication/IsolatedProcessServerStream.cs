using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace hhnl.ProcessIsolation.Communication
{
    public class IsolatedProcessServerStream : IIsolatedPipeStream, IDisposable, IAsyncDisposable
    {
        private readonly NamedPipeServerStream _serverStream;

        public IsolatedProcessServerStream(string pipeName)
        {
            var pipePath = $"LOCAL\\{pipeName}";

            _serverStream = new NamedPipeServerStream(pipePath,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);
        }

        public IsolatedProcessServerStream(NamedPipeServerStream serverStream)
        {
            _serverStream = serverStream;
        }

        public ValueTask DisposeAsync()
        {
            return _serverStream.DisposeAsync();
        }

        public void Dispose()
        {
            _serverStream.Dispose();
        }

        public PipeStream Stream => _serverStream;

        public Task StartAsync(CancellationToken token = default)
        {
            return WaitForConnectionAsync(token);
        }

        public Task StopAsync()
        {
            Disconnect();
            return Task.CompletedTask;
        }

        public Task WaitForConnectionAsync()
        {
            return _serverStream.WaitForConnectionAsync();
        }

        public Task WaitForConnectionAsync(CancellationToken cancellationToken)
        {
            return _serverStream.WaitForConnectionAsync(cancellationToken);
        }

        public void Disconnect()
        {
            _serverStream.Disconnect();
        }
    }
}