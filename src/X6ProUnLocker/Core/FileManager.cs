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
            // Запрос привилегии SeTakeOwnershipPrivilege
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

            // Получаем SID текущего пользователя
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            byte[] sid = new byte[identity.User.BinaryLength];
            identity.User.GetBinaryForm(sid, 0);

            // Устанавливаем владельца
            int result = WinApiNative.SetNamedSecurityInfo(
                filePath,
                WinApiNative.SE_OBJECT_TYPE.SE_FILE_OBJECT,
                WinApiNative.SECURITY_INFORMATION.OWNER_SECURITY_INFORMATION,
                sid,
                null,
                null,
                null);

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

                // Восстанавливаем разрешения (DACL) как у оригинала
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
            // Используем SDDL для установки DACL: SYSTEM: полный, Администраторы: полный, Users: чтение/выполнение
            string sddl = "D:(A;;FA;;;SY)(A;;FA;;;BA)(A;;FRFX;;;BU)";
            IntPtr pSD;
            uint size;
            if (WinApiNative.ConvertStringSecurityDescriptorToSecurityDescriptor(sddl, 1, out pSD, out size))
            {
                WinApiNative.SetNamedSecurityInfo(
                    filePath,
                    WinApiNative.SE_OBJECT_TYPE.SE_FILE_OBJECT,
                    WinApiNative.SECURITY_INFORMATION.DACL_SECURITY_INFORMATION,
                    null,
                    null,
                    pSD,
                    null);
                Marshal.FreeHGlobal(pSD);
            }
        }
    }
}