using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace X6ProUnLocker.Core
{
    public class ProcessInfo
    {
        public int Pid { get; set; }
        public string Name { get; set; } = "";
        public string? Path { get; set; }
        public long Memory { get; set; } // in bytes
    }

    public static class ProcessManager
    {
        public static List<ProcessInfo> GetProcessList()
        {
            var list = new List<ProcessInfo>();
            IntPtr snapshot = WinApiNative.CreateToolhelp32Snapshot(0x00000002, 0); // TH32CS_SNAPPROCESS
            if (snapshot == IntPtr.Zero) return list;

            var entry = new WinApiNative.PROCESSENTRY32();
            entry.dwSize = (uint)Marshal.SizeOf(entry);

            if (WinApiNative.Process32First(snapshot, ref entry))
            {
                do
                {
                    var info = new ProcessInfo
                    {
                        Pid = (int)entry.th32ProcessID,
                        Name = entry.szExeFile
                    };
                    // Try to open process to get path
                    IntPtr hProcess = WinApiNative.OpenProcess(0x0400 | 0x0010, false, entry.th32ProcessID); // PROCESS_QUERY_INFORMATION | PROCESS_VM_READ
                    if (hProcess != IntPtr.Zero)
                    {
                        var sb = new System.Text.StringBuilder(260);
                        if (WinApiNative.GetModuleFileNameEx(hProcess, IntPtr.Zero, sb, (uint)sb.Capacity) > 0)
                            info.Path = sb.ToString();
                        WinApiNative.CloseHandle(hProcess);
                    }
                    // Get memory via Process class
                    try
                    {
                        var proc = Process.GetProcessById(info.Pid);
                        info.Memory = proc.WorkingSet64;
                    }
                    catch { }
                    list.Add(info);
                } while (WinApiNative.Process32Next(snapshot, ref entry));
            }
            WinApiNative.CloseHandle(snapshot);
            return list;
        }
    }
}