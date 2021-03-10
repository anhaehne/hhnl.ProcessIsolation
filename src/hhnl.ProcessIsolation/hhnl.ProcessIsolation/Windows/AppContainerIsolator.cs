using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NtApiDotNet;
using NtApiDotNet.Win32;
using NtApiDotNet.Win32.Security;
using NtApiDotNet.Win32.Security.Authorization;

namespace hhnl.ProcessIsolation.Windows
{
    public class AppContainerIsolator : IProcessIsolator
    {
        private static readonly Lazy<NtJob> _childProcessCascadeCloseJob = new Lazy<NtJob>(
            () =>
            {
                var job = NtJob.Create();
                job.SetLimitFlags(JobObjectLimitFlags.KillOnJobClose);

                return job;
            });

        public bool IsCurrentProcessIsolated()
        {
            using NtToken token = NtToken.OpenProcessToken();

            return token.AppContainer;
        }

        /// <summary>
        /// Starts the process isolated.
        /// </summary>
        /// <param name="isolationIdentifier">Name identifier for the isolation. This has to be unique. Under windows this is the name of the AppContainerIsolator.</param>
        /// <param name="path">The path to the executable.</param>
        /// <param name="commandLineArguments">The command line arguments.</param>
        /// <param name="networkPermissions">The network permissions the process should have.</param>
        /// <param name="attachToCurrentProcess">
        /// If set to <c>true</c> the started process will be terminated when the current
        /// process exits.
        /// </param>
        /// <param name="fileAccess">The extended file access. Allows for custom file and folder access rights.</param>
        /// <param name="makeApplicationDirectoryReadable">
        /// By default the folder containing the executable will be made available to the process to allow loading dependencies.
        /// Set this to false to suppress this behaviour.
        /// </param>
        /// <param name="workingDirectory">
        /// The working directory of the process.
        /// </param>
        /// <returns>
        /// The process object. Needs to be disposed.
        /// </returns>
        /// <exception cref="ArgumentException">$"Couldn't resolve directory for '{path}'.</exception>
        public IIsolatedProcess StartIsolatedProcess(
            string isolationIdentifier,
            string path,
            string[]? commandLineArguments = null,
            NetworkPermissions networkPermissions = NetworkPermissions.None,
            bool attachToCurrentProcess = true,
            IEnumerable<FileAccess>? fileAccess = null,
            bool makeApplicationDirectoryReadable = true,
            string? workingDirectory = null)
        {
            string applicationName = Path.GetFileNameWithoutExtension(path);

            var container = AppContainerProfile.Create(
                isolationIdentifier,
                $"{applicationName} Container ({isolationIdentifier})",
                $"Application container for {applicationName}");

            var config = new Win32ProcessConfig
            {
                ApplicationName = path,
                CommandLine = commandLineArguments is not null ? string.Join(" ", commandLineArguments) : string.Empty,
                ChildProcessMitigations = ChildProcessMitigationFlags.Restricted,
                AppContainerSid = container.Sid,
                TerminateOnDispose = true,
                CurrentDirectory = workingDirectory is null ? null : Path.GetFullPath(workingDirectory),
            };

            // Allow the process to access it's own files.
            if (makeApplicationDirectoryReadable)
            {
                var appDirectory = Path.GetDirectoryName(path) ??
                                   throw new ArgumentException($"Couldn't resolve directory for '{path}'.");
                AllowFileAccess(container, appDirectory, FileAccessRights.GenericRead);
            }

            // Apply user folder and file permissions
            if (fileAccess is not null)
            {
                foreach (var cur in fileAccess)
                {
                    if (!Directory.Exists(cur.Path) && !File.Exists(cur.Path))
                        throw new ArgumentException($"The file or folder '{cur.Path}' does not exist.");

                    AllowFileAccess(
                        container,
                        cur.Path,
                        (FileAccessRights)cur.AccessRight);
                }
            }

            // Apply network networkPermissions
            if ((networkPermissions & NetworkPermissions.LocalNetwork) != 0)
                config.AddCapability(KnownSids.CapabilityPrivateNetworkClientServer);

            if ((networkPermissions & NetworkPermissions.Internet) != 0)
                config.AddCapability(KnownSids.CapabilityInternetClient);

            var process = Win32Process.CreateProcess(config);

            // Make sure the new process gets killed when the current process stops.
            if (attachToCurrentProcess)
                _childProcessCascadeCloseJob.Value.AssignProcess(process.Process);

            return new IsolatedWin32Process(process, container);
        }

        private static void AllowFileAccess(AppContainerProfile container, string folder, FileAccessRights accessRights)
        {
            var securityInfo = Win32Security.GetSecurityInfo(
                folder,
                SeObjectType.File,
                SecurityInformation.Dacl);

            var existingAce = securityInfo.Dacl.FirstOrDefault(d => d.Sid == container.Sid);

            if (existingAce is not null &&
                existingAce.Type == AceType.Allowed &&
                existingAce.Mask == accessRights &&
                existingAce.Flags == (AceFlags.ContainerInherit | AceFlags.ObjectInherit))
            {
                // Ace already exists.
                return;
            }

            var ace = new Ace(
                AceType.Allowed,
                AceFlags.ContainerInherit | AceFlags.ObjectInherit,
                accessRights,
                container.Sid);

            securityInfo.AddAce(ace);

            Win32Security.SetSecurityInfo(
                folder,
                SeObjectType.File,
                SecurityInformation.Dacl,
                securityInfo,
                true);
        }
    }
}