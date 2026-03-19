using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;

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
            if (!WinApiNative.LookupPrivilegeValue(null, WinApiNative.SE_TAKE_OWNERSHIP_NAME, out luid))
            {
                WinApiNative.CloseHandle(hToken);
                return false;
            }

            WinApiNative.TOKEN_PRIVILEGES tp = new WinApiNative.TOKEN_PRIVILEGES
            {
                PrivilegeCount = 1,
                Luid = luid,
                Attributes = WinApiNative.SE_PRIVILEGE_ENABLED
            };

            if (!WinApiNative.AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
            {
                WinApiNative.CloseHandle(hToken);
                return false;
            }

            WinApiNative.CloseHandle(hToken);

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            byte[] sid = new byte[identity.User.BinaryLength];
            identity.User.GetBinaryForm(sid, 0);

            GCHandle handle = GCHandle.Alloc(sid, GCHandleType.Pinned);
            IntPtr sidPtr = handle.AddrOfPinnedObject();

            int result = WinApiNative.SetNamedSecurityInfo(
                filePath,
                WinApiNative.SE_OBJECT_TYPE.SE_FILE_OBJECT,
                WinApiNative.SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION,
                sidPtr,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

            handle.Free();
            return result == 0;
        }

        public static bool ReplaceSystemUtility(string utilityPath, string replacementPath, Action<string, System.Windows.Media.Color> log)
        {
            if (!File.Exists(utilityPath) || !File.Exists(replacementPath))
                return false;

            string backupPath = utilityPath + ".bak";
            try
            {
                if (!TakeOwnership(utilityPath))
                {
                    log?.Invoke("❌ Failed to take ownership of " + utilityPath, System.Windows.Media.Colors.Red);
                    return false;
                }

                File.Copy(utilityPath, backupPath, true);
                log?.Invoke($"✅ Backup created: {backupPath}", System.Windows.Media.Colors.LightGreen);

                File.Delete(utilityPath);
                File.Copy(replacementPath, utilityPath);

                RestoreFilePermissions(utilityPath);

                return true;
            }
            catch (Exception ex)
            {
                log?.Invoke($"❌ Error: {ex.Message}", System.Windows.Media.Colors.Red);
                return false;
            }
        }

        public static void RestoreFilePermissions(string filePath)
        {
            string sddl = "D:(A;;FA;;;SY)(A;;FA;;;BA)(A;;FRFX;;;BU)";
            IntPtr pSD;
            uint size;
            if (WinApiNative.ConvertStringSecurityDescriptorToSecurityDescriptor(sddl, 1, out pSD, out size))
            {
                WinApiNative.SetNamedSecurityInfo(
                    filePath,
                    WinApiNative.SE_OBJECT_TYPE.SE_FILE_OBJECT,
                    WinApiNative.SECURITY_INFORMATION.DACL_SECURITY_INFORMATION,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    pSD,
                    IntPtr.Zero);
                Marshal.FreeHGlobal(pSD);
            }
        }
    }
}