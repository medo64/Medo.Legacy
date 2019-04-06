/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2017-11-13: Fixed CallbackOnCollectedDelegate exception due to GC collecting callback function.
//2017-11-12: Initial version.


using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Medo.Win32 {
    /// <summary>
    /// Handling low-level keyboard hook.
    /// </summary>
    public class LowLevelKeyboardHook : IDisposable {

        /// <summary>
        /// Create a new instance.
        /// </summary>
        public LowLevelKeyboardHook() {
        }


        /// <summary>
        /// Starts the keyboard hook.
        /// </summary>
        /// <exception cref="Win32Exception"></exception>
        public void Hook() {
            if (HookHandle.IsInvalid || HookHandle.IsClosed) {
                if (LowLevelKeyboardProcedure == null) { //needs to be in variable so GC doesn't collect it
                    LowLevelKeyboardProcedure = new NativeMethods.LowLevelKeyboardProc(KeyboardHookProc);
                }
                HookHandle = NativeMethods.SetWindowsHookEx(
                    idHook: NativeMethods.WH_KEYBOARD_LL,
                    lpfn: LowLevelKeyboardProcedure,
                    hMod: IntPtr.Zero,
                    dwThreadId: 0);

                if (HookHandle.IsInvalid) { throw new Win32Exception(); }
            }
        }

        /// <summary>
        /// Releases the keyboard hook.
        /// </summary>
        public void Unhook() {
            if ((HookHandle.IsInvalid == false) && (HookHandle.IsClosed == false)) {
                HookHandle.Close();
            }
        }


        /// <summary>
        /// Event called when hook receives a callback.
        /// </summary>
        public event EventHandler<LowLevelKeyboardHookCallbackEventArgs> KeyboardCallback;

        private void OnKeyboardCallback(LowLevelKeyboardHookCallbackEventArgs e) {
            KeyboardCallback?.Invoke(this, e);
        }


        private NativeMethods.WindowsHookSafeHandle HookHandle = new NativeMethods.WindowsHookSafeHandle();
        private NativeMethods.LowLevelKeyboardProc LowLevelKeyboardProcedure;

        private IntPtr KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam) {
            var keyboardHookStruct = (NativeMethods.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(NativeMethods.KBDLLHOOKSTRUCT));

            if (nCode == NativeMethods.HC_ACTION) {
                OnKeyboardCallback(new LowLevelKeyboardHookCallbackEventArgs(
                    virtualKeyCode: keyboardHookStruct.vkCode,
                    scanCode: keyboardHookStruct.scanCode,
                    flags: keyboardHookStruct.flags));
                return NativeMethods.CallNextHookEx(HookHandle, nCode, wParam, lParam);
            } else {
                return NativeMethods.CallNextHookEx(HookHandle, nCode, wParam, lParam);
            }
        }


        #region IDispose

        /// <summary>
        /// Destroys the instance.
        /// </summary>
        ~LowLevelKeyboardHook() {
            Dispose(false);
        }

        /// <summary>
        /// Disposes resources in use.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes resources in use.
        /// </summary>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                LowLevelKeyboardProcedure = null; //let GC collect it now
            }
            Unhook();
        }

        #endregion


        private static class NativeMethods {
#pragma warning disable IDE0049 // Simplify Names

            internal const Int32 WH_KEYBOARD_LL = 13;
            internal const Int32 HC_ACTION = 0;


            [StructLayout(LayoutKind.Sequential)]
            internal struct KBDLLHOOKSTRUCT {
                public Int32 vkCode;
                public Int32 scanCode;
                public Int32 flags;
                public Int32 time;
                public UIntPtr dwExtraInfo;
            }


            [DebuggerDisplay("handle")]
            internal class WindowsHookSafeHandle : SafeHandleZeroOrMinusOneIsInvalid {
                public WindowsHookSafeHandle()
                    : base(true) {
                }

                protected override bool ReleaseHandle() {
                    return UnhookWindowsHookEx(base.handle);
                }
            }


            internal delegate IntPtr LowLevelKeyboardProc(Int32 nCode, IntPtr wParam, IntPtr lParam);


            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            internal static extern WindowsHookSafeHandle SetWindowsHookEx(Int32 idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, Int32 dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            [return: MarshalAsAttribute(UnmanagedType.Bool)]
            private static extern Boolean UnhookWindowsHookEx(IntPtr idHook);

            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            internal static extern IntPtr CallNextHookEx(WindowsHookSafeHandle hhk, Int32 nCode, IntPtr wParam, IntPtr lParam);

#pragma warning restore IDE0049 // Simplify Names
        }

    }


    /// <summary>
    /// Low-level keyboard event arguments.
    /// </summary>
    public class LowLevelKeyboardHookCallbackEventArgs : EventArgs {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="virtualKeyCode">Virtual key code.</param>
        /// <param name="scanCode">Hardware scan code</param>
        /// <param name="flags">Key flags.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags", Justification = "Flags is used by Win32 on which this is based on.")]
        public LowLevelKeyboardHookCallbackEventArgs(int virtualKeyCode, int scanCode, int flags) {
            VirtualKeyCode = virtualKeyCode;
            ScanCode = scanCode;
            IsExtended = (flags & 0x01) != 0;
            IsInjectedFromLowerIntegrityLevel = (flags & 0x02) != 0;
            IsInjected = (flags & 0x10) != 0;
            IsAltPressed = (flags & 0x20) != 0;
            IsPressed = (flags & 0x80) == 0;
        }


        /// <summary>
        /// Gets a virtual key code.
        /// </summary>
        public int VirtualKeyCode { get; }

        /// <summary>
        /// Gets a hardware scan code for the key.
        /// </summary>
        public int ScanCode { get; }

        /// <summary>
        /// Specifies whether the key is an extended key, such as a function key or a key on the numeric keypad.
        /// </summary>
        public bool IsExtended { get; }

        /// <summary>
        /// Specifies whether the event was injected from a process running at lower integrity level.
        /// </summary>
        public bool IsInjectedFromLowerIntegrityLevel { get; }

        /// <summary>
        /// Specifies whether the event was injected.
        /// </summary>
        public bool IsInjected { get; }

        /// <summary>
        /// Specifies whether ALT key was pressed.
        /// </summary>
        public bool IsAltPressed { get; }

        /// <summary>
        /// Specifies whether key was pressed.
        /// </summary>
        public bool IsPressed { get; }
    }
}
