using System;
using System.Buffers;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace hhnl.ProcessIsolation.Communication
{
    public class IsolatedProcessChannel<TSend, TReceive> : IDisposable, IAsyncDisposable
    {
        private const int HeaderSize = sizeof(int);
        private readonly ITransportSerializer _serializer;
        private readonly IIsolatedPipeStream _stream;
        private readonly Channel<TSend> _sendChannel;
        private CancellationTokenSource? _cts;
        private Task? _sendTask;

        public IsolatedProcessChannel(IIsolatedPipeStream stream, ITransportSerializer serializer, int sendMessageBuffer = 32)
        {
            _stream = stream;
            _serializer = serializer;
            _sendChannel = Channel.CreateBounded<TSend>(sendMessageBuffer);
        }

        public async Task<TReceive> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            byte[]? headerBuffer = null;
            byte[]? messageBuffer = null;

            try
            {
                // Read header
                headerBuffer = ArrayPool<byte>.Shared.Rent(HeaderSize);
                var headerReadBytes = await _stream.Stream.ReadAsync(headerBuffer, 0, HeaderSize, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
                
                // Verify header
                if (headerReadBytes != HeaderSize)
                    throw new InvalidOperationException("Unable to read message header.");
                var messageLength = BitConverter.ToInt32(headerBuffer);
                if (messageLength < 1)
                    throw new InvalidOperationException($"Invalid message length {messageLength}");
                
                // Read message
                messageBuffer = ArrayPool<byte>.Shared.Rent(messageLength);
                var bytesRead = await _stream.Stream.ReadAsync(messageBuffer, 0, messageLength, cancellationToken);
                
                cancellationToken.ThrowIfCancellationRequested();
                
                if(bytesRead != messageLength)
                    throw new InvalidOperationException($"Could not read message til the end.");

                return _serializer.Deserialize<TReceive>(((ReadOnlySpan<byte>)messageBuffer).Slice(0, messageLength));
            }
            finally
            {
                if (headerBuffer is not null)
                    ArrayPool<byte>.Shared.Return(headerBuffer);

                if (messageBuffer is not null)
                    ArrayPool<byte>.Shared.Return(messageBuffer);
            }
        }

        public ValueTask EnqueueMessageAsync(TSend message, CancellationToken cancellationToken = default)
        {
            return _sendChannel.Writer.WriteAsync(message, cancellationToken);
        }

        private async Task SendInternalAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var message = await _sendChannel.Reader.ReadAsync(cancellationToken);
                
                var (messageLength, messageBuffer) = _serializer.Serialize(message);
            
                // Send header
                var header = BitConverter.GetBytes(messageLength);
                await _stream.Stream.WriteAsync(header, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;
            
                // Send message
                await _stream.Stream.WriteAsync(messageBuffer, 0, messageLength, cancellationToken);
            }
        }
        
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_cts is not null)
                throw new InvalidOperationException("Channel already started.");
            
            _cts = new CancellationTokenSource();
            
            await _stream.StartAsync(cancellationToken);
            _sendTask = SendInternalAsync(_cts.Token);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        { 
            if (_cts is null)
                throw new InvalidOperationException("Channel not yet started.");
            
            _cts?.Cancel();

            if (_sendTask is not null)
            {
                try
                {
                    await _sendTask;
                }
                catch (OperationCanceledException)
                {
                }
            }
            
            await _stream.StopAsync(cancellationToken);
            
            _cts?.Dispose();
            _cts = null;
        }

        public void Dispose()
        {
            _stream.Dispose();
            _cts?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _stream.DisposeAsync();
            _cts?.Dispose();
        }
    }

    public class IsolatedProcessChannel<TSendReceive> : IsolatedProcessChannel<TSendReceive, TSendReceive>
    {
        public IsolatedProcessChannel(IIsolatedPipeStream stream, ITransportSerializer serializer) : base(stream, serializer)
        {
        }
    }
}