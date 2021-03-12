using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace hhnl.ProcessIsolation.Communication
{
    public interface IIsolatedPipeStream : IDisposable, IAsyncDisposable
    {
        public PipeStream Stream { get; }

        public Task StartAsync(CancellationToken token);
        
        public Task StopAsync(CancellationToken token);
    }
}