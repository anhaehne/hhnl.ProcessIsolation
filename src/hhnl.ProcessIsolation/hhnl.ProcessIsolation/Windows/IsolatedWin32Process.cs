using System.Diagnostics;
using NtApiDotNet.Win32;

namespace hhnl.ProcessIsolation.Windows
{
    public class IsolatedWin32Process : IIsolatedProcess
    {
        private readonly Win32Process _process;
        private readonly AppContainerProfile _appContainer;

        public IsolatedWin32Process(Win32Process process, AppContainerProfile appContainer)
        {
            _process = process;
            _appContainer = appContainer;
            Process = Process.GetProcessById(process.Pid);
        }

        public Process Process { get; }

        public string SecurityIdentifier => _appContainer.Sid.ToString();

        public int SessionId => _process.Process.SessionId;

        public void Dispose()
        {
            _process.Dispose();
            _appContainer.Dispose();
        }
    }
}