/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2017-07-16: Added to library.


using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace Medo.Windows.Forms {

    /// <summary>
    /// Folder selection dialog.
    /// </summary>
    public class OpenFolderDialog : IDisposable {

        /// <summary>
        /// Gets/sets folder in which dialog will be open.
        /// </summary>
        public string InitialFolder { get; set; }

        /// <summary>
        /// Gets/sets directory in which dialog will be open if there is no recent directory available.
        /// </summary>
        public string DefaultFolder { get; set; }

        /// <summary>
        /// Gets selected folder.
        /// </summary>
        public string Folder { get; private set; }


        /// <summary>
        /// Shows dialog.
        /// </summary>
        /// <param name="owner">Owner window.</param>
        public DialogResult ShowDialog(IWin32Window owner) {
            if (Environment.OSVersion.Version.Major >= 6) {
                return ShowVistaDialog(owner);
            } else {
                return ShowLegacyDialog(owner);
            }
        }

        private DialogResult ShowVistaDialog(IWin32Window owner) {
            var frm = (NativeMethods.IFileDialog)(new NativeMethods.FileOpenDialogRCW());
            frm.GetOptions(out var options);
            options |= NativeMethods.FOS_PICKFOLDERS | NativeMethods.FOS_FORCEFILESYSTEM | NativeMethods.FOS_NOVALIDATE | NativeMethods.FOS_NOTESTFILECREATE | NativeMethods.FOS_DONTADDTORECENT;
            frm.SetOptions(options);

            if (InitialFolder != null) {
                var riid = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"); //IShellItem
                if (NativeMethods.SHCreateItemFromParsingName(InitialFolder, IntPtr.Zero, ref riid, out var directoryShellItem) == NativeMethods.S_OK) {
                    frm.SetFolder(directoryShellItem);
                }
            }

            if (DefaultFolder != null) {
                var riid = new Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"); //IShellItem
                if (NativeMethods.SHCreateItemFromParsingName(DefaultFolder, IntPtr.Zero, ref riid, out var directoryShellItem) == NativeMethods.S_OK) {
                    frm.SetDefaultFolder(directoryShellItem);
                }
            }

            var ownerHandle = (owner != null) ? owner.Handle : IntPtr.Zero;
            if (frm.Show(ownerHandle) == NativeMethods.S_OK) {
                if (frm.GetResult(out var shellItem) == NativeMethods.S_OK) {
                    if (shellItem.GetDisplayName(NativeMethods.SIGDN_FILESYSPATH, out var pszString) == NativeMethods.S_OK) {
                        if (pszString != IntPtr.Zero) {
                            try {
                                Folder = Marshal.PtrToStringAuto(pszString);
                                return DialogResult.OK;
                            } finally {
                                Marshal.FreeCoTaskMem(pszString);
                            }
                        }
                    }
                }
            }
            return DialogResult.Cancel;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.FileDialog.set_Title(System.String)", Justification = "No localization is planned.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.FileDialog.set_Filter(System.String)", Justification = "This field is not user visible.")]
        private DialogResult ShowLegacyDialog(IWin32Window owner) {
            using (var frm = new SaveFileDialog()) { //using SaveFileDialog so empty folder can be selected
                frm.CheckFileExists = false;
                frm.CheckPathExists = true;
                frm.CreatePrompt = false;
                frm.Filter = "*|" + Guid.Empty.ToString();
                frm.FileName = "any";
                if (InitialFolder != null) { frm.InitialDirectory = InitialFolder; }
                frm.OverwritePrompt = false;
                frm.Title = "Select Folder";
                frm.ValidateNames = false;
                if (frm.ShowDialog(owner) == DialogResult.OK) {
                    Folder = Path.GetDirectoryName(frm.FileName);
                    return DialogResult.OK;
                } else {
                    return DialogResult.Cancel;
                }
            }
        }


        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources.</param>
        protected virtual void Dispose(bool disposing) {
            //just here to have possibility of Using statement.
        }
    }

    internal static class NativeMethods {
#pragma warning disable IDE0049 // Simplify Names

        #region Constants

        public const UInt32 FOS_PICKFOLDERS = 0x00000020;
        public const UInt32 FOS_FORCEFILESYSTEM = 0x00000040;
        public const UInt32 FOS_NOVALIDATE = 0x00000100;
        public const UInt32 FOS_NOTESTFILECREATE = 0x00010000;
        public const UInt32 FOS_DONTADDTORECENT = 0x02000000;

        public const UInt32 S_OK = 0x0000;

        public const UInt32 SIGDN_FILESYSPATH = 0x80058000;

        #endregion


        #region COM

        [ComImport, ClassInterface(ClassInterfaceType.None), TypeLibType(TypeLibTypeFlags.FCanCreate), Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
        internal class FileOpenDialogRCW { }


        [ComImport(), Guid("42F85136-DB7E-439C-85F1-E4075D135FC8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IFileDialog {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [PreserveSig()]
            UInt32 Show([In, Optional] IntPtr hwndOwner); //IModalWindow 


            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 SetFileTypes([In] UInt32 cFileTypes, [In, MarshalAs(UnmanagedType.LPArray)] IntPtr rgFilterSpec);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 SetFileTypeIndex([In] UInt32 iFileType);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 GetFileTypeIndex(out UInt32 piFileType);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 Advise([In, MarshalAs(UnmanagedType.Interface)] IntPtr pfde, out uint pdwCookie);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 Unadvise([In] UInt32 dwCookie);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 SetOptions([In] UInt32 fos);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 GetOptions(out UInt32 fos);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            void SetDefaultFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 SetFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 GetFolder([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] String pszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 GetFileName([MarshalAs(UnmanagedType.LPWStr)] out String pszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] String pszTitle);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] String pszText);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] String pszLabel);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 GetResult([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 AddPlace([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, UInt32 fdap);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] String pszDefaultExtension);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 Close([MarshalAs(UnmanagedType.Error)] UInt32 hr);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 SetClientGuid([In] ref Guid guid);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 ClearClientData();

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);
        }


        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IShellItem {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 BindToHandler([In] IntPtr pbc, [In] ref Guid rbhid, [In] ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out IntPtr ppvOut);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 GetParent([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 GetDisplayName([In] UInt32 sigdnName, out IntPtr ppszName);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 GetAttributes([In] UInt32 sfgaoMask, out UInt32 psfgaoAttribs);

            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            UInt32 Compare([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, [In] UInt32 hint, out Int32 piOrder);
        }

        #endregion


        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern Int32 SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] String pszPath, IntPtr pbc, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out IShellItem ppv);

#pragma warning restore IDE0049 // Simplify Names
    }
}
