using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace X6ProUnLocker.Core
{
    public class ScanResultEventArgs : EventArgs
    {
        public string FilePath { get; set; }
        public bool IsMalware { get; set; }
        public string Message { get; set; }
    }

    public class VirusScanner
    {
        public event EventHandler<ScanResultEventArgs> ScanCompleted;

        private static readonly string[] KnownMalwareHashes = {
            "e99a18c428cb38d5f260853678922e03",
            "5d41402abc4b2a76b9719d911017c592",
            "098f6bcd4621d373cade4e832627b4f6"
        };

        public async void ScanFileAsync(string filePath)
        {
            var result = await Task.Run(() => ScanFile(filePath));
            ScanCompleted?.Invoke(this, result);
        }

        private ScanResultEventArgs ScanFile(string filePath)
        {
            if (!File.Exists(filePath))
                return new ScanResultEventArgs { FilePath = filePath, IsMalware = false, Message = "File does not exist" };

            string hash = ComputeSha256(filePath);
            if (KnownMalwareHashes.Contains(hash))
                return new ScanResultEventArgs { FilePath = filePath, IsMalware = true, Message = "Known malware detected!" };

            if (!HasValidSignature(filePath))
                return new ScanResultEventArgs { FilePath = filePath, IsMalware = false, Message = "File is not digitally signed (may be dangerous)" };

            return new ScanResultEventArgs { FilePath = filePath, IsMalware = false, Message = "File checked, no threats found" };
        }

        private string ComputeSha256(string filePath)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            byte[] hash = sha.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private bool HasValidSignature(string filePath)
        {
            try
            {
                var fileInfo = new WinApiNative.WINTRUST_FILE_INFO
                {
                    cbStruct = (uint)Marshal.SizeOf<WinApiNative.WINTRUST_FILE_INFO>(),
                    pcwszFilePath = filePath,
                    hFile = IntPtr.Zero,
                    pgKnownSubject = IntPtr.Zero
                };

                var guidAction = new Guid("{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}"); // WINTRUST_ACTION_GENERIC_VERIFY_V2

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