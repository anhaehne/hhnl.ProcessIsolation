using System;
using System.Threading.Tasks;

namespace hhnl.ProcessIsolation.Communication
{
    public interface ITransportSerializer
    {
        (int Length, byte[] buffer) Serialize<T>(T message);

        T Deserialize<T>(ReadOnlySpan<byte> bytes);
    }
}