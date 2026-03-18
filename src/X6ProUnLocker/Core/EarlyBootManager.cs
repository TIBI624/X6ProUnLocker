using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Windows.Media;

namespace X6ProUnLocker.Core
{
    public static class EarlyBootManager
    {
        public static void RegisterEarlyBootService()
        {
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            try
            {
                // Используем sc.exe для создания службы (можно и через P/Invoke CreateService)
                System.Diagnostics.Process.Start("sc", $"create X6ProUnLockerEarlyBoot binPath= \"{exePath}\" start= auto");
                // В реальном коде лучше использовать ServiceController или CreateService
            }
            catch (Exception ex)
            {
                // Логирование будет через внешний метод
            }
        }

        public static void CheckCriticalSystemFiles(Action<string, Color> log)
        {
            string[] criticalFiles = { "cmd.exe", "sethc.exe", "utilman.exe", "explorer.exe", "winlogon.exe" };
            string systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);

            foreach (var file in criticalFiles)
            {
                string path = Path.Combine(systemDir, file);
                if (!File.Exists(path))
                {
                    log($"⚠️ Critical file missing: {file}", Colors.Yellow);
                }
                else
                {
                    // Проверка подписи через VirusScanner (можно вынести общий метод)
                    bool valid = HasValidSignature(path);
                    if (!valid)
                        log($"⚠️ File without valid signature: {file}", Colors.Yellow);
                }
            }
        }

        private static bool HasValidSignature(string filePath)
        {
            // Используем тот же метод, что и в VirusScanner
            try
            {
                var fileInfo = new WinApiNative.WINTRUST_FILE_INFO
                {
                    cbStruct = (uint)Marshal.SizeOf<WinApiNative.WINTRUST_FILE_INFO>(),
                    pcwszFilePath = filePath,
                    hFile = IntPtr.Zero,
                    pgKnownSubject = IntPtr.Zero
                };

                var guidAction = new Guid("{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}");
                var trustData = new WinApiNative.WINTRUST_DATA
                {
                    cbStruct = (uint)Marshal.SizeOf<WinApiNative.WINTRUST_DATA>(),
                    dwUIChoice = WinApiNative.WTD_UI_NONE,
                    fdwRevocationChecks = WinApiNative.WTD_REVOKE_NONE,
                    dwUnionChoice = WinApiNative.WTD_CHOICE_FILE,
                    pFile = Marshal.AllocHGlobal(Marshal.SizeOf(fileInfo)),
                    dwStateAction = WinApiNative.WTD_STATEACTION_VERIFY,
                    dwProvFlags = WinApiNative.WTD_SAFER_FLAG,
                    dwUIContext = 0
                };

                Marshal.StructureToPtr(fileInfo, trustData.pFile, false);
                int result = WinApiNative.WinVerifyTrust(IntPtr.Zero, ref guidAction, ref trustData);
                trustData.dwStateAction = WinApiNative.WTD_STATEACTION_CLOSE;
                WinApiNative.WinVerifyTrust(IntPtr.Zero, ref guidAction, ref trustData);
                Marshal.FreeHGlobal(trustData.pFile);
                return result == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}