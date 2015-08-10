using System;
using System.Runtime.InteropServices;
using System.Security;

namespace VideoModule
{
    class RegisterDeviceNotifications : IDisposable
    {
        #region Definitions
#pragma warning disable 169
        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        private class DEV_BROADCAST_HDR
        {
            public int dbch_size;
#pragma warning disable 649
            public int dbch_devicetype;
#pragma warning restore 649
            public int dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        // ReSharper disable once InconsistentNaming
        private class DEV_BROADCAST_DEVICEINTERFACE
        {
            // ReSharper disable once NotAccessedField.Local
            public int dbcc_size;
#pragma warning disable 414
            public int dbcc_devicetype;
#pragma warning restore 414
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            public char dbcc_name;
        }
#pragma warning restore 169

        [DllImport("User32.dll",
            CharSet = CharSet.Unicode,
            ExactSpelling = true,
            EntryPoint = "RegisterDeviceNotificationW",
            SetLastError = true),
        SuppressUnmanagedCodeSecurity]
        private static extern IntPtr RegisterDeviceNotification(
            IntPtr hDlg,
            [MarshalAs(UnmanagedType.LPStruct)] DEV_BROADCAST_DEVICEINTERFACE di,
            int dwFlags
            );

        [DllImport("User32.dll", ExactSpelling = true, SetLastError = true), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterDeviceNotification(
            IntPtr hDlg
            );

        // ReSharper disable InconsistentNaming
        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        private const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        // ReSharper restore InconsistentNaming

        #endregion

        // Handle of the notification.  Used by unregister
        IntPtr hdevnotify;

        // Category of events
        readonly Guid category;

        public RegisterDeviceNotifications(IntPtr hWnd, Guid gCat)
        {
            category = gCat;

            var di = new DEV_BROADCAST_DEVICEINTERFACE();

            // Register to be notified of events of category gCat
            di.dbcc_size = Marshal.SizeOf(di);
            di.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
            di.dbcc_classguid = gCat;

            hdevnotify = RegisterDeviceNotification(
                hWnd,
                di,
                DEVICE_NOTIFY_WINDOW_HANDLE
                );

            // If it failed, throw an exception
            if (hdevnotify == IntPtr.Zero)
            {
                var i = unchecked((int)0x80070000);
                i += Marshal.GetLastWin32Error();
                throw new COMException("Failed to RegisterDeviceNotifications", i);
            }
        }

        public void Dispose()
        {
            if (hdevnotify != IntPtr.Zero)
            {
                UnregisterDeviceNotification(hdevnotify);
                hdevnotify = IntPtr.Zero;
            }
        }

        // Static routine to parse out the device type from the IntPtr received in WndProc
        public bool CheckEventDetails(IntPtr pReason, IntPtr pHdr)
        {
            var iValue = pReason.ToInt32();

            // Check the event type
            if (iValue != DBT_DEVICEREMOVECOMPLETE && iValue != DBT_DEVICEARRIVAL)
                return false;

            // Do we have device details yet?
            if (pHdr == IntPtr.Zero)
                return false;

            // Parse the first chunk
            var pbh = new DEV_BROADCAST_HDR();
            Marshal.PtrToStructure(pHdr, pbh);

            // Check the device type
            if (pbh.dbch_devicetype != DBT_DEVTYP_DEVICEINTERFACE)
                return false;

            // Only parse this if the right device type
            var pdi = new DEV_BROADCAST_DEVICEINTERFACE();
            Marshal.PtrToStructure(pHdr, pdi);

            return (pdi.dbcc_classguid == category);
        }

        // Static routine to parse out the Symbolic name from the IntPtr received in WndProc
        public static string ParseDeviceSymbolicName(IntPtr pHdr)
        {
            var ip = Marshal.OffsetOf(typeof(DEV_BROADCAST_DEVICEINTERFACE), "dbcc_name");
            return Marshal.PtrToStringUni(pHdr + (ip.ToInt32()));
        }
    }
}
