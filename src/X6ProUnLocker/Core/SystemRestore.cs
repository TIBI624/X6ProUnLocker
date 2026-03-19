using System.IO;

namespace X6ProUnLocker.Core
{
    public static class SystemRestore
    {
        public static void RestoreOriginalUtilities(string systemDir, System.Action<string, System.Windows.Media.Color> log)
        {
            string[] utils = { "cmd.exe", "sethc.exe", "utilman.exe" };
            foreach (var util in utils)
            {
                string orig = Path.Combine(systemDir, util);
                string backup = orig + ".bak";
                if (File.Exists(backup))
                {
                    if (FileManager.TakeOwnership(orig))
                    {
                        File.Delete(orig);
                        File.Copy(backup, orig);
                        File.Delete(backup);
                        log?.Invoke($"✅ Restored {util}", System.Windows.Media.Colors.LightGreen);
                    }
                    else
                    {
                        log?.Invoke($"❌ Failed to take ownership for {util}", System.Windows.Media.Colors.Red);
                    }
                }
            }
        }

        public static void RestoreSystemFonts()
        {
            // Stub: copy fonts from backup or install via FontManager
        }
    }
}