using System;

namespace hhnl.ProcessIsolation
{
    /// <summary>
    /// Specifies the file or folder access right.
    /// </summary>
    public class FileAccess
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileAccess" /> class.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="accessRights">The file access rights.</param>
        public FileAccess(string path, Right accessRights)
        {
            Path = path;
            AccessRight = accessRights;
        }

        /// <summary>
        /// Gets or sets the path to the file or folder.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; }

        /// <summary>
        /// Gets the file access rights for the file or folder.
        /// </summary>
        /// <value>
        /// The file access rights.
        /// </value>
        public Right AccessRight { get; }

        [Flags]
        public enum Right : uint
        {
            Read = 2147483648,
            Write = 1073741824
        }
    }
}