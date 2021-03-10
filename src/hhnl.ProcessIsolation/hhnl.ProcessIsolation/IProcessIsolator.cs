using System;
using System.Collections.Generic;

namespace hhnl.ProcessIsolation
{
    public interface IProcessIsolator
    {
        bool IsCurrentProcessIsolated();

        /// <summary>
        /// Starts the process isolated.
        /// </summary>
        /// <param name="isolationIdentifier">
        /// Name identifier for the isolation. This has to be unique. Under windows this is the
        /// name of the AppContainerIsolator.
        /// </param>
        /// <param name="path">The path to the executable.</param>
        /// <param name="commandLineArguments">The command line arguments.</param>
        /// <param name="networkPermissions">The network permissions the process should have.</param>
        /// <param name="attachToCurrentProcess">
        /// If set to <c>true</c> the started process will be terminated when the current
        /// process exits.
        /// </param>
        /// <param name="fileAccess">The file access. Allows to specify file and folder access rights.</param>
        /// <returns>
        /// The process object. Needs to be disposed.
        /// </returns>
        /// <param name="makeApplicationDirectoryReadable">
        /// By default the folder containing the executable will be made available to the process to allow loading dependencies.
        /// Set this to false to suppress this behaviour.
        /// </param>
        /// <param name="workingDirectory">
        /// The working directory of the process.
        /// </param>
        /// <exception cref="ArgumentException">$"Couldn't resolve directory for '{path}'.</exception>
        IIsolatedProcess StartIsolatedProcess(
            string isolationIdentifier,
            string path,
            string[]? commandLineArguments = null,
            NetworkPermissions networkPermissions = NetworkPermissions.None,
            bool attachToCurrentProcess = true,
            IEnumerable<FileAccess>? fileAccess = null,
            bool makeApplicationDirectoryReadable = true,
            string? workingDirectory = null);
    }
}