using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using TaskScheduler = Microsoft.Win32.TaskScheduler.TaskService;

namespace X6ProUnLocker.Core
{
    public static class AutostartManager
    {
        private const string TaskName = "X6ProUnLockerStartup";
        private const string ServiceName = "X6ProUnLockerAutoStart";

        public static void AddToRegistryCurrentUser(string appPath)
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            key?.SetValue("X6ProUnLocker", appPath);
        }

        public static void AddToRegistryLocalMachine(string appPath)
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            key?.SetValue("X6ProUnLocker", appPath);
        }

        public static void AddToStartupFolder(string appPath)
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string linkPath = Path.Combine(folder, "X6ProUnLocker.lnk");
            CreateShortcut(appPath, linkPath);
        }

        private static void CreateShortcut(string targetPath, string shortcutPath)
        {
            Type? t = Type.GetTypeFromProgID("WScript.Shell");
            if (t == null) return;
            dynamic? shell = Activator.CreateInstance(t);
            if (shell == null) return;
            var shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            shortcut.Save();
            Marshal.ReleaseComObject(shortcut);
            Marshal.ReleaseComObject(shell);
        }

        public static void AddToTaskScheduler(string appPath)
        {
            using (var ts = new TaskScheduler())
            {
                var task = ts.NewTask();
                task.RegistrationInfo.Description = "X6ProUnLocker autorun";
                task.Triggers.Add(new Microsoft.Win32.TaskScheduler.LogonTrigger());
                task.Actions.Add(new Microsoft.Win32.TaskScheduler.ExecAction(appPath));
                ts.RootFolder.RegisterTaskDefinition(TaskName, task);
            }
        }

        public static void AddToService(string appPath)
        {
            System.Diagnostics.Process.Start("sc", $"create {ServiceName} binPath= \"{appPath}\" start= auto");
        }

        public static void AddToWinIni(string appPath)
        {
            string winIniPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "win.ini");
            if (!File.Exists(winIniPath)) return;
            var lines = new System.Collections.Generic.List<string>(File.ReadAllLines(winIniPath));
            bool inWindows = false;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim().Equals("[windows]", StringComparison.OrdinalIgnoreCase))
                {
                    inWindows = true;
                    continue;
                }
                if (inWindows && lines[i].Trim().StartsWith("load="))
                {
                    lines[i] += "," + appPath;
                    File.WriteAllLines(winIniPath, lines);
                    return;
                }
            }
            lines.Add("[windows]");
            lines.Add($"load={appPath}");
            File.WriteAllLines(winIniPath, lines);
        }

        public static void RemoveAll()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                key?.DeleteValue("X6ProUnLocker", false);
            using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                key?.DeleteValue("X6ProUnLocker", false);

            string folder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string link = Path.Combine(folder, "X6ProUnLocker.lnk");
            if (File.Exists(link)) File.Delete(link);

            try
            {
                using (var ts = new TaskScheduler())
                {
                    ts.RootFolder.DeleteTask(TaskName, false);
                }
            }
            catch { }

            try
            {
                ServiceController sc = new ServiceController(ServiceName);
                if (sc.Status != ServiceControllerStatus.Stopped)
                    sc.Stop();
                System.Diagnostics.Process.Start("sc", $"delete {ServiceName}");
            }
            catch { }

            string winIniPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "win.ini");
            if (File.Exists(winIniPath))
            {
                var lines = new System.Collections.Generic.List<string>(File.ReadAllLines(winIniPath));
                bool inWindows = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Trim().Equals("[windows]", StringComparison.OrdinalIgnoreCase))
                    {
                        inWindows = true;
                        continue;
                    }
                    if (inWindows && lines[i].Trim().StartsWith("load="))
                    {
                        string load = lines[i].Substring(5);
                        var parts = load.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        var newParts = new System.Collections.Generic.List<string>();
                        foreach (var p in parts)
                        {
                            if (!p.Trim().Equals(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, StringComparison.OrdinalIgnoreCase))
                                newParts.Add(p);
                        }
                        lines[i] = "load=" + string.Join(",", newParts);
                        File.WriteAllLines(winIniPath, lines);
                        break;
                    }
                }
            }
        }
    }
}