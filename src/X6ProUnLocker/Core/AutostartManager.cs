using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using System.Text;
using TaskScheduler = Microsoft.Win32.TaskScheduler.TaskService;

namespace X6ProUnLocker.Core
{
    public static class AutostartManager
    {
        private const string TaskNamePrefix = "X6PU_";
        private const string ServiceNamePrefix = "X6PU_";

        public static List<AutoStartEntry> GetEntries()
        {
            var entries = new List<AutoStartEntry>();
            try
            {
                // HKCU
                using var cu = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                if (cu != null) foreach (var v in cu.GetValueNames()) entries.Add(new AutoStartEntry { Name = v, Path = cu.GetValue(v)?.ToString() ?? "", Location = "HKCU", Type = "Registry" });

                // HKLM
                using var lm = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                if (lm != null) foreach (var v in lm.GetValueNames()) entries.Add(new AutoStartEntry { Name = v, Path = lm.GetValue(v)?.ToString() ?? "", Location = "HKLM", Type = "Registry" });

                // Startup Folder
                string sf = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                if (Directory.Exists(sf)) foreach (var f in Directory.GetFiles(sf, "*.lnk")) entries.Add(new AutoStartEntry { Name = Path.GetFileName(f), Path = f, Location = sf, Type = "Shortcut" });

                // Task Scheduler (basic scan)
                using var ts = new TaskScheduler();
                foreach (var t in ts.RootFolder.GetTasks())
                    if (t.Name.StartsWith(TaskNamePrefix) || t.Definition.Actions.Count > 0)
                        entries.Add(new AutoStartEntry { Name = t.Name, Path = t.Definition.Actions[0]?.Path ?? "", Location = "TaskScheduler", Type = "Scheduled Task" });
            }
            catch { }
            return entries;
        }

        public static bool AddEntry(string name, string path, string location = "Registry")
        {
            try
            {
                switch (location)
                {
                    case "Registry":
                        Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true)?.SetValue(name, path);
                        break;
                    case "HKLM":
                        Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true)?.SetValue(name, path);
                        break;
                    case "Startup":
                        CreateShortcut(path, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), name + ".lnk"));
                        break;
                    case "TaskScheduler":
                        using (var ts = new TaskScheduler())
                        {
                            var task = ts.NewTask();
                            task.RegistrationInfo.Description = name;
                            task.Triggers.Add(new Microsoft.Win32.TaskScheduler.LogonTrigger());
                            task.Actions.Add(new Microsoft.Win32.TaskScheduler.ExecAction(path));
                            ts.RootFolder.RegisterTaskDefinition(TaskNamePrefix + name, task);
                        }
                        break;
                }
                return true;
            }
            catch { return false; }
        }

        public static bool RemoveEntry(string name, string location, string path = "")
        {
            try
            {
                switch (location)
                {
                    case "HKCU":
                        Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true)?.DeleteValue(name, false);
                        break;
                    case "HKLM":
                        Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true)?.DeleteValue(name, false);
                        break;
                    case "Startup":
                        string lnk = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), name + ".lnk");
                        if (File.Exists(lnk)) File.Delete(lnk);
                        break;
                    case "TaskScheduler":
                        using (var ts = new TaskScheduler()) ts.RootFolder.DeleteTask(name, false);
                        break;
                    case "Service":
                        System.Diagnostics.Process.Start("sc", $"stop {name}").WaitForExit();
                        System.Diagnostics.Process.Start("sc", $"delete {name}").WaitForExit();
                        break;
                }
                return true;
            }
            catch { return false; }
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
            Marshal.ReleaseCOMObject(shell);
        }
    }
}