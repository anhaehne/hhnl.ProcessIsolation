using System;
using System.Diagnostics;

namespace hhnl.ProcessIsolation
{
    public interface IIsolatedProcess : IDisposable
    {
        Process Process { get; }
    }
}