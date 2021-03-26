using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace hhnl.ProcessIsolation.Communication
{
    public class IsolatedProcessClientStream : IIsolatedPipeStream, IDisposable, IAsyncDisposable
    {
        private readonly NamedPipeClientStream _clientStream;

        public IsolatedProcessClientStream(IIsolatedProcess process, string pipeName)
        {
            var fullPipeName =
                $"\\Sessions\\{process.SessionId}\\AppContainerNamedObjects\\{process.SecurityIdentifier}\\{pipeName}";
            _clientStream = new NamedPipeClientStream(".",
                fullPipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous);
        }

        public IsolatedProcessClientStream(NamedPipeClientStream clientStream)
        {
            _clientStream = clientStream;
        }

        public PipeStream Stream => _clientStream;

        public Task StartAsync(CancellationToken token = default)
        {
            return ConnectAsync(token);
        }

        public Task StopAsync()
        {
            _clientStream.Close();
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return _clientStream.DisposeAsync();
        }

        public void Dispose()
        {
            _clientStream.Dispose();
        }

        public Task ConnectAsync()
        {
            return _clientStream.ConnectAsync();
        }

        public Task ConnectAsync(int timeout)
        {
            return _clientStream.ConnectAsync(timeout);
        }

        public Task ConnectAsync(CancellationToken cancellationToken)
        {
            return _clientStream.ConnectAsync(cancellationToken);
        }

        public Task ConnectAsync(int timeout, CancellationToken cancellationToken)
        {
            return _clientStream.ConnectAsync(timeout, cancellationToken);
        }

        public void Close()
        {
            _clientStream.Close();
        }
    }
}