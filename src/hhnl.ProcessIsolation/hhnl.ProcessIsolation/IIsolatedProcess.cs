using System;
using System.Diagnostics;

namespace hhnl.ProcessIsolation
{
    public interface IIsolatedProcess : IDisposable
    {
        /// <summary>
        /// Gets the process.
        /// </summary>
        /// <value>
        /// The process.
        /// </value>
        Process Process { get; }

        /// <summary>
        /// Gets the security identifier of the isolation context.
        /// </summary>
        /// <value>
        /// The security identifier.
        /// </value>
        string SecurityIdentifier { get; }

        /// <summary>
        /// Gets the session identifier of the process.
        /// </summary>
        /// <value>
        /// The session identifier.
        /// </value>
        int SessionId { get; }
    }
}