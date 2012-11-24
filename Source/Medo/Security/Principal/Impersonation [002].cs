//Copyright (c) 2008 Josip Medved <jmedved@jmedved.com>

//2008-04-29: Inital release.
//2012-11-24: Suppressing bogus CA5122 warning (http://connect.microsoft.com/VisualStudio/feedback/details/729254/bogus-ca5122-warning-about-p-invoke-declarations-should-not-be-safe-critical).


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;

namespace Medo.Security.Principal {

    /// <summary>
    /// Static class for impersonation of other users.
    /// This class is thread-safe.
    /// </summary>
    public static class Impersonation {

        private static Dictionary<int, TransferBag> _impersonationPerThreadID = new Dictionary<int, TransferBag>();
        private static readonly object _syncRoot = new object();


        /// <summary>
        /// Impersonates user.
        /// Returns true if impersonation was successful.
        /// If there is already impersonation in progress, user is reverted to default before new impersonation is performed (there is no nested impersonation).
        /// </summary>
        /// <param name="userName">User's name.</param>
        /// <param name="domain">User's domain. If local computer is desired, "." can be used.</param>
        /// <param name="password">User's password.</param>
        [SecurityPermission(SecurityAction.LinkDemand)]
        public static bool Impersonate(string userName, string domain, string password) {
            return Impersonate(userName, domain, password, TokenImpersonationLevel.Impersonation);
        }

        /// <summary>
        /// Impersonates user.
        /// Returns true if impersonation was successful.
        /// If there is already impersonation in progress, user is reverted to default before new impersonation is performed (there is no nested impersonation).
        /// </summary>
        /// <param name="userName">User's name.</param>
        /// <param name="domain">User's domain. If local computer is desired, "." can be used.</param>
        /// <param name="password">User's password.</param>
        /// <param name="impersonationLevel"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle", Justification = "This method is needed in order to properly call into Win32 API.")]
        [SecurityPermission(SecurityAction.LinkDemand)]
        public static bool Impersonate(string userName, string domain, string password, TokenImpersonationLevel impersonationLevel) {
            lock (_syncRoot) {
                int threadID = Thread.CurrentThread.ManagedThreadId;

                if (IsCurrentlyImpersonated) { Revert(); }


                TokenHandle newUserToken = new TokenHandle();
                bool luRet = NativeMethods.LogonUser(userName, domain, password, NativeMethods.LOGON32_LOGON_INTERACTIVE, NativeMethods.LOGON32_PROVIDER_DEFAULT, ref newUserToken);
                if (luRet == false) {
                    newUserToken.Close();
                    return false;
                }

                TokenHandle duplicatedUserToken = new TokenHandle();
                bool dtRet = NativeMethods.DuplicateToken(newUserToken, (Int32)impersonationLevel, ref duplicatedUserToken);
                if (dtRet == false) {
                    duplicatedUserToken.Close();
                    newUserToken.Close();
                    return false;
                }

                try {
                    WindowsIdentity newIdentity = new WindowsIdentity(duplicatedUserToken.DangerousGetHandle());
                    WindowsImpersonationContext impersonationContext = newIdentity.Impersonate();

                    _impersonationPerThreadID.Add(threadID, new TransferBag(newUserToken, duplicatedUserToken, impersonationContext));

                    return true;
                } finally {
                    duplicatedUserToken.Close();
                    newUserToken.Close();
                }
            }
        }


        /// <summary>
        /// Removes impersonation from current thread.
        /// Returns true if sucessful or false if user was not impersonated.
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand)]
        public static bool Revert() {
            lock (_syncRoot) {
                int threadID = Thread.CurrentThread.ManagedThreadId;

                if (IsCurrentlyImpersonated) {
                    TransferBag valueBag = _impersonationPerThreadID[threadID];
                    try {
                        valueBag.WindowsImpersonationContext.Undo();
                    } finally {
                        valueBag.LogonUserHandle.Close();
                        valueBag.LogonUserDuplicatedHandle.Close();
                    }
                    _impersonationPerThreadID.Remove(threadID);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets whether user on current thread is currently impersonated.
        /// </summary>
        public static bool IsCurrentlyImpersonated {
            get {
                lock (_syncRoot) {
                    return _impersonationPerThreadID.ContainsKey(Thread.CurrentThread.ManagedThreadId);
                }
            }
        }


        private struct TransferBag {

            public TransferBag(TokenHandle logonUserHandle, TokenHandle logonUserDuplicatedHandle, WindowsImpersonationContext windowsImpersonationContext) {
                this.LogonUserHandle = logonUserHandle;
                this.LogonUserDuplicatedHandle = logonUserDuplicatedHandle;
                this.WindowsImpersonationContext = windowsImpersonationContext;
            }

            public TokenHandle LogonUserHandle;
            public TokenHandle LogonUserDuplicatedHandle;
            public WindowsImpersonationContext WindowsImpersonationContext;

        }



        private class TokenHandle : SafeHandle {

            [SecurityPermission(SecurityAction.LinkDemand)]
            public TokenHandle()
                : base(IntPtr.Zero, true) {
            }

            public override bool IsInvalid {
                get { return base.handle != IntPtr.Zero; }
            }

            protected override bool ReleaseHandle() {
                return NativeMethods.CloseHandle(base.handle);
            }

        }

        private static class NativeMethods {

            internal const Int32 LOGON32_LOGON_INTERACTIVE = 2;
            internal const Int32 LOGON32_PROVIDER_DEFAULT = 0;
            internal const Int32 SECURITY_IMPERSONATION_LEVEL_IMPERSONATION = 2;


            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA5122:PInvokesShouldNotBeSafeCriticalFxCopRule", Justification = "Warning is bogus.")]
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseHandle(IntPtr hObject);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA5122:PInvokesShouldNotBeSafeCriticalFxCopRule", Justification = "Warning is bogus.")]
            [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DuplicateToken(TokenHandle ExistingTokenHandle, Int32 ImpersonationLevel, ref TokenHandle DuplicatedTokenHandle);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA5122:PInvokesShouldNotBeSafeCriticalFxCopRule", Justification = "Warning is bogus.")]
            [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, Int32 dwLogonType, Int32 dwLogonProvider, ref TokenHandle phToken);

        }

    }

}
