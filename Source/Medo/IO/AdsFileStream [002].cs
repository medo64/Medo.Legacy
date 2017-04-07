//Josip Medved <jmedved@jmedved.com>   www.medo64.com

//2012-11-24: Suppressing bogus CA5122 warning (http://connect.microsoft.com/VisualStudio/feedback/details/729254/bogus-ca5122-warning-about-p-invoke-declarations-should-not-be-safe-critical).
//2010-02-07: Initial version.


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;


namespace Medo.IO {

    /// <summary>
    /// Support for NTFS Alternate data streams.
    /// </summary>
    public class AdsFileStream : Stream {

        #region Static

        /// <summary>
        /// Enumerates all alternate data stream names associated with file.
        /// Original data stream name is NOT returned.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current object will encapsulate.</param>
        /// <exception cref="System.ArgumentNullException">Parameter "path" cannot be null.</exception>
        /// <exception cref="System.ComponentModel.Win32Exception">Invalid handle value. -or- Failed retrieving information.</exception>
        /// <exception cref="System.NotSupportedException">Operation is not supported on current OS version (minimum is Windows Vista).</exception>
        /// <exception cref="System.ComponentModel.Win32Exception">Failed retrieving information.</exception>
        /// <remarks>Iterating NTFS Streams by Stephen Toub (http://msdn.microsoft.com/en-us/magazine/cc163677.aspx)</remarks>
        public static IEnumerable<string> GetStreamNames(string path) {
            if (path == null) throw new ArgumentNullException("path", "Parameter \"path\" cannot be null.");

            var data = new NativeMethods.WIN32_FIND_STREAM_DATA();

            NativeMethods.SafeFindStreamHandle handle = null;
            try {
                try {
                    handle = NativeMethods.FindFirstStreamW(path, NativeMethods.STREAM_INFO_LEVELS.FindStreamInfoStandard, ref  data, (uint)0);
                    if (handle.IsInvalid) throw new Win32Exception(Marshal.GetLastWin32Error(), "Invalid handle value.");
                } catch (EntryPointNotFoundException) {
                    throw new NotSupportedException("Operation is not supported on current OS version (minimum is Windows Vista).");
                }

                while (NativeMethods.FindNextStreamW(handle, ref data)) {
                    if (data.cStreamName.StartsWith(":", StringComparison.OrdinalIgnoreCase) && data.cStreamName.EndsWith(":$DATA", StringComparison.OrdinalIgnoreCase)) {
                        yield return data.cStreamName.Substring(1, data.cStreamName.Length - 7);
                    }
                }

                int lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error != NativeMethods.ERROR_HANDLE_EOF) {
                    throw new Win32Exception(lastWin32Error, "Failed retrieving information.");
                }
            } finally {
                if (handle != null) {
                    handle.Dispose();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">A relative or absolute path for the file.</param>
        /// <param name="streamName">Name of NTFS alternate data stream to be deleted.</param>
        /// <exception cref="System.ArgumentNullException">Parameter cannot be null.</exception>
        /// <exception cref="System.ComponentModel.Win32Exception">Function failed.</exception>
        public static void DeleteStream(string path, string streamName) {
            if (path == null) { throw new ArgumentNullException("path", "Parameter \"path\" cannot be null."); }
            if (streamName == null) { throw new ArgumentNullException("streamName", "Parameter \"streamName\" cannot be null."); }

            if (!NativeMethods.DeleteFileW(@"\\?\" + path + ":" + streamName)) {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Function failed.");
            }
        }

        #endregion



        /// <summary>
        /// Underlying FileStream.
        /// </summary>
        protected FileStream FileStream { get; set; }


        /// <summary>
        /// Creates mew instance.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current object will encapsulate.</param>
        /// <param name="streamName">Name of NTFS alternate data stream or null if primary stream is to be accessed.</param>
        /// <param name="mode">A System.IO.FileMode constant that determines how to open or create the file.</param>
        /// <param name="access">A System.IO.FileAccess constant that determines how the file can be accessed by the FileStream object. This gets the System.IO.FileStream.CanRead and System.IO.FileStream.CanWrite properties of the FileStream object. System.IO.FileStream.CanSeek is true if path specifies a disk file.</param>
        /// <param name="share">A System.IO.FileShare constant that determines how the file will be shared by processes.</param>
        /// <exception cref="System.ArgumentNullException">Path is null.</exception>
        /// <exception cref="System.ArgumentException">Path is an empty string (""), contains only white space, or contains one or more invalid characters. -or-path refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in an NTFS environment.</exception>
        /// <exception cref="System.NotSupportedException">Path refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">BufferSize is negative or zero.-or- mode, access, or share contain an invalid value.</exception>
        /// <exception cref="System.IO.FileNotFoundException">The file cannot be found, such as when mode is FileMode.Truncate or FileMode.Open, and the file specified by path does not exist. The file must already exist in these modes.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs, such as specifying FileMode.CreateNew and the file specified by path already exists.-or-The stream has been closed.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="System.UnauthorizedAccessException">The access requested is not permitted by the operating system for the specified path, such as when access is Write or ReadWrite and the file or directory is set for read-only access. -or-System.IO.FileOptions.Encrypted is specified for options, but file encryption is not supported on the current platform.</exception>
        /// <exception cref="System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
        /// <exception cref="System.ComponentModel.Win32Exception">Invalid handle value.</exception>
        public AdsFileStream(string path, string streamName, FileMode mode, FileAccess access, FileShare share)
            : base() {
            if (streamName == null) { //no ADS support
                this.FileStream = new FileStream(path, mode, access, share);
            } else {
                uint dwDesiredAccess = 0;
                switch (access) {
                    case FileAccess.Read:
                        dwDesiredAccess = NativeMethods.GENERIC_READ;
                        break;
                    case FileAccess.Write:
                        dwDesiredAccess = NativeMethods.GENERIC_WRITE;
                        break;
                    case FileAccess.ReadWrite:
                        dwDesiredAccess = NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE;
                        break;
                }

                uint dwShareMode = 0;
                switch (share) {
                    case FileShare.Delete:
                        dwShareMode = NativeMethods.FILE_SHARE_DELETE;
                        break;
                    case FileShare.Inheritable:
                        dwShareMode = 0;
                        break;
                    case FileShare.None:
                        dwShareMode = 0;
                        break;
                    case FileShare.Read:
                        dwShareMode = NativeMethods.FILE_SHARE_READ;
                        break;
                    case FileShare.ReadWrite:
                        dwShareMode = NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE;
                        break;
                    case FileShare.Write:
                        dwShareMode = NativeMethods.FILE_SHARE_WRITE;
                        break;
                }

                uint dwCreationDisposition = 0;
                switch (mode) {
                    case FileMode.Append:
                        dwCreationDisposition = NativeMethods.OPEN_ALWAYS;
                        break;
                    case FileMode.Create:
                        dwCreationDisposition = NativeMethods.CREATE_ALWAYS;
                        break;
                    case FileMode.CreateNew:
                        dwCreationDisposition = NativeMethods.CREATE_NEW;
                        break;
                    case FileMode.Open:
                        dwCreationDisposition = NativeMethods.OPEN_EXISTING;
                        break;
                    case FileMode.OpenOrCreate:
                        dwCreationDisposition = NativeMethods.OPEN_ALWAYS;
                        break;
                    case FileMode.Truncate:
                        dwCreationDisposition = NativeMethods.TRUNCATE_EXISTING;
                        break;
                }

                uint dwFlagsAndAttributes = NativeMethods.FILE_FLAG_OPEN_NO_RECALL;

                var filePtr = NativeMethods.CreateFileW(path + ":" + streamName, dwDesiredAccess, dwShareMode, IntPtr.Zero, dwCreationDisposition, dwFlagsAndAttributes, IntPtr.Zero);
                if (filePtr.IsInvalid) {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Invalid handle value.");
                }
                this.FileStream = new FileStream(filePtr, access);
                if (mode == FileMode.Append) { this.FileStream.Seek(0, SeekOrigin.End); }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead {
            get { return this.FileStream.CanRead; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek {
            get { return this.FileStream.CanSeek; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite {
            get { return this.FileStream.CanWrite; }
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the file system.
        /// </summary>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        public override void Flush() {
            this.FileStream.Flush();
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <exception cref="System.NotSupportedException">System.IO.FileStream.CanSeek for this stream is false.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs, such as the file being closed.</exception>
        public override long Length {
            get { return this.FileStream.Length; }
        }

        /// <summary>
        /// Gets or sets the current position of this stream.
        /// </summary>
        /// <exception cref="System.NotSupportedException">The stream does not support seeking.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs. - or -The position was set to a very large value beyond the end of the stream in Windows 98 or earlier.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Attempted to set the position to a negative value.</exception>
        /// <exception cref="System.IO.EndOfStreamException">Attempted seeking past the end of a stream that does not support this.</exception>
        public override long Position {
            get {
                return this.FileStream.Position;
            }
            set {
                this.FileStream.Position = value;
            }
        }

        /// <summary>
        /// Reads a block of bytes from the stream and writes the data in a given buffer.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The byte offset in array at which the read bytes will be placed.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes read into the buffer. This might be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached.</returns>
        /// <exception cref="System.ArgumentNullException">Array is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Offset or count is negative.</exception>
        /// <exception cref="System.NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.ArgumentException">Offset and count describe an invalid range in array.</exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override int Read(byte[] buffer, int offset, int count) {
            return this.FileStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Sets the current position of this stream to the given value.
        /// </summary>
        /// <param name="offset">The point relative to origin from which to begin seeking.</param>
        /// <param name="origin">Specifies the beginning, the end, or the current position as a reference point for origin, using a value of type System.IO.SeekOrigin.</param>
        /// <returns>The new position in the stream.</returns>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.NotSupportedException">The stream does not support seeking, such as if the FileStream is constructed from a pipe or console output.</exception>
        /// <exception cref="System.ArgumentException">Attempted seeking before the beginning of the stream.</exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override long Seek(long offset, System.IO.SeekOrigin origin) {
            return this.FileStream.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length of this stream to the given value.
        /// </summary>
        /// <param name="value">The new length of the stream.</param>
        /// <exception cref="System.IO.IOException">An I/O error has occurred</exception>
        /// <exception cref="System.NotSupportedException">The stream does not support both writing and seeking.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Attempted to set the value parameter to less than 0.</exception>
        public override void SetLength(long value) {
            this.SetLength(value);
        }

        /// <summary>
        /// Writes a block of bytes to this stream using data from a buffer.
        /// </summary>
        /// <param name="buffer">The buffer containing data to write to the stream.</param>
        /// <param name="offset">The zero-based byte offset in array at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="System.ArgumentNullException">Array is null.</exception>
        /// <exception cref="System.ArgumentException">Offset and count describe an invalid range in array.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Offset or count is negative.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs. - or -Another thread may have caused an unexpected change in the position of the operating system's file handle.</exception>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="System.NotSupportedException">The current stream instance does not support writing.</exception>
        public override void Write(byte[] buffer, int offset, int count) {
            this.FileStream.Write(buffer, offset, count);
        }


        /// <summary>
        /// Releases the unmanaged resources used by the System.IO.FileStream and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        protected override void Dispose(bool disposing) {
            this.FileStream.SafeFileHandle.Close();
            this.FileStream.Dispose();
            base.Dispose(disposing);
        }




        private static class NativeMethods {

            public const uint GENERIC_READ = 0x80000000;
            public const uint GENERIC_WRITE = 0x40000000;

            public const uint FILE_SHARE_DELETE = 4;
            public const uint FILE_SHARE_WRITE = 2;
            public const uint FILE_SHARE_READ = 1;

            public const uint CREATE_ALWAYS = 2;
            public const uint CREATE_NEW = 1;
            public const uint OPEN_ALWAYS = 4;
            public const uint OPEN_EXISTING = 3;
            public const uint TRUNCATE_EXISTING = 5;

            public const uint FILE_FLAG_OPEN_NO_RECALL = 0x00100000;


            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA5122:PInvokesShouldNotBeSafeCriticalFxCopRule", Justification = "Warning is bogus.")]
            [DllImportAttribute("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode)]
            public static extern SafeFileHandle CreateFileW([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPWStr)] string lpFileName, uint dwDesiredAccess, uint dwShareMode, [InAttribute()] IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, [InAttribute()] IntPtr hTemplateFile);




            public const int ERROR_HANDLE_EOF = 38;


            public enum STREAM_INFO_LEVELS {
                FindStreamInfoStandard,
                FindStreamInfoMaxInfoLevel,
            }


            internal sealed class SafeFindStreamHandle : SafeHandleZeroOrMinusOneIsInvalid {
                private SafeFindStreamHandle() : base(true) { }

                protected override bool ReleaseHandle() {
                    return FindClose(this.handle);
                }

                //[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
                [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA5122:PInvokesShouldNotBeSafeCriticalFxCopRule", Justification = "Warning is bogus.")]
                [DllImportAttribute("kernel32.dll", EntryPoint = "FindClose", CharSet = CharSet.Unicode)]
                [return: MarshalAsAttribute(UnmanagedType.Bool)]
                public static extern bool FindClose(IntPtr hFindFile);

            }


            [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct WIN32_FIND_STREAM_DATA {
                public Int64 StreamSize;

                [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 296)]
                public string cStreamName;
            }


            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA5122:PInvokesShouldNotBeSafeCriticalFxCopRule", Justification = "Warning is bogus.")]
            [DllImportAttribute("Kernel32.dll", EntryPoint = "FindFirstStreamW", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern SafeFindStreamHandle FindFirstStreamW([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPWStr)] string lpFileName, STREAM_INFO_LEVELS InfoLevel, ref WIN32_FIND_STREAM_DATA lpFindStreamData, uint dwFlags);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA5122:PInvokesShouldNotBeSafeCriticalFxCopRule", Justification = "Warning is bogus.")]
            [DllImportAttribute("Kernel32.dll", EntryPoint = "FindNextStreamW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAsAttribute(UnmanagedType.Bool)]
            public static extern bool FindNextStreamW([InAttribute()] SafeFindStreamHandle hFindStream, ref WIN32_FIND_STREAM_DATA lpFindStreamData);



            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA5122:PInvokesShouldNotBeSafeCriticalFxCopRule", Justification = "Warning is bogus.")]
            [DllImportAttribute("Kernel32.dll", EntryPoint = "DeleteFileW", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAsAttribute(UnmanagedType.Bool)]
            public static extern bool DeleteFileW([InAttribute()] [MarshalAsAttribute(UnmanagedType.LPWStr)] string lpFileName);

        }

    }
}
