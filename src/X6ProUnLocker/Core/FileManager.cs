using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Media;

namespace X6ProUnLocker.Core
{
    public static class FileManager
    {
        public static bool TakeOwnership(string filePath)
        {
            IntPtr hToken;
            if (!WinApiNative.OpenProcessToken(System.Diagnostics.Process.GetCurrentProcess().Handle,
                WinApiNative.TOKEN_ADJUST_PRIVILEGES | WinApiNative.TOKEN_QUERY, out hToken))
                return false;

            long luid;
            // исправлен вызов: передаём null через string? параметр
            if (!WinApiNative.LookupPrivilegeValue(null, WinApiNative.SE_TAKE_OWNERSHIP_NAME, out luid))
            {
                WinApiNative.CloseHandle(hToken);
                return false;
            }

            var tp = new WinApiNative.TOKEN_PRIVILEGES { PrivilegeCount = 1, Luid = luid, Attributes = WinApiNative.SE_PRIVILEGE_ENABLED };
            if (!WinApiNative.AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
            {
                WinApiNative.CloseHandle(hToken);
                return false;
            }
            WinApiNative.CloseHandle(hToken);

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            // identity.User не будет null в нормальном контексте, но успокоим компилятор
            if (identity.User == null)
                return false;
            byte[] sid = new byte[identity.User.BinaryLength];
            identity.User.GetBinaryForm(sid, 0);
            GCHandle handle = GCHandle.Alloc(sid, GCHandleType.Pinned);
            int res = WinApiNative.SetNamedSecurityInfo(filePath, WinApiNative.SE_OBJECT_TYPE.SE_FILE_OBJECT,
                WinApiNative.SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION, handle.AddrOfPinnedObject(), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            handle.Free();
            return res == 0;
        }

        public static bool ReplaceSystemUtility(string utilityPath, string replacementPath, Action<string, Color> log)
        {
            if (!File.Exists(utilityPath) || !File.Exists(replacementPath))
                return false;

            string backupPath = utilityPath + ".bak";
            try
            {
                if (!TakeOwnership(utilityPath))
                {
                    log?.Invoke(LanguageManager.Get("StatusError").Replace("{0}", "Failed to take ownership: " + utilityPath), Colors.Red);
                    return false;
                }
                if (File.Exists(backupPath)) File.Delete(backupPath);
                File.Copy(utilityPath, backupPath, true);
                File.Delete(utilityPath);
                File.Copy(replacementPath, utilityPath);
                RestoreFilePermissions(utilityPath);
                log?.Invoke(LanguageManager.Get("StatusSuccess").Replace("{0}", $"Backup created & {Path.GetFileName(utilityPath)} replaced"), Colors.LightGreen);
                return true;
            }
            catch (Exception ex)
            {
                log?.Invoke(LanguageManager.Get("StatusError").Replace("{0}", ex.Message), Colors.Red);
                return false;
            }
        }

        public static void RestoreFilePermissions(string filePath)
        {
            string sddl = "D:(A;;FA;;;SY)(A;;FA;;;BA)(A;;FRFX;;;BU)";
            if (WinApiNative.ConvertStringSecurityDescriptorToSecurityDescriptor(sddl, 1, out IntPtr pSD, out uint size))
            {
                WinApiNative.SetNamedSecurityInfo(filePath, WinApiNative.SE_OBJECT_TYPE.SE_FILE_OBJECT,
                    WinApiNative.SECURITY_INFORMATION.DACL_SECURITY_INFORMATION, IntPtr.Zero, IntPtr.Zero, pSD, IntPtr.Zero);
                Marshal.FreeHGlobal(pSD);
            }
        }
    }
}