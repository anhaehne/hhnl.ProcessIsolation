using System.Diagnostics;
using NtApiDotNet.Win32;

namespace hhnl.ProcessIsolation.Windows
{
    public class IsolatedWin32Process : IIsolatedProcess
    {
        private readonly Win32Process _process;

        public IsolatedWin32Process(Win32Process process)
        {
            _process = process;
            Process = Process.GetProcessById(process.Pid);
        }

        public Process Process { get; }

        public void Dispose()
        {
            _process.Dispose();
        }
    }
}