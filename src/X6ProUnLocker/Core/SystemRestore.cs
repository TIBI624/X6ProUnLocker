using System.IO;

namespace X6ProUnLocker.Core
{
    public static class SystemRestore
    {
        public static void RestoreOriginalUtilities(string systemDir)
        {
            string[] utils = { "cmd.exe", "sethc.exe", "utilman.exe" };
            foreach (var util in utils)
            {
                string orig = Path.Combine(systemDir, util);
                string backup = orig + ".bak";
                if (File.Exists(backup))
                {
                    FileManager.TakeOwnership(orig);
                    File.Delete(orig);
                    File.Copy(backup, orig);
                    File.Delete(backup);
                }
            }
        }

        public static void RestoreSystemFonts()
        {
            // Stub: copy fonts from backup or install via FontManager
        }
    }
}